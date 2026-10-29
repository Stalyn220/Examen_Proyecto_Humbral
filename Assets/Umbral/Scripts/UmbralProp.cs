using UnityEngine;

namespace Umbral
{
    // Componente configurable para objetos del nivel: puertas, modulos, interruptores,
    // puntos de control, peligros y otros elementos con los que puede interactuar el jugador.
    public enum PropKind { Module, Lever, Reactor, Portal, Checkpoint, Hint, GravityZone, Hazard, Door, CircuitSwitch }

    public sealed class UmbralProp : MonoBehaviour
    {
        // Configuracion por objeto. index identifica modulos/puertas y circuit enlaza interruptores con puertas.
        public PropKind kind;
        public int index;
        public string circuit;
        public Vector2 checkpointOffset;
        [TextArea] public string message;
        public SpriteRenderer indicator;
        public Collider2D barrier;
        public bool Activated { get; private set; }

        // Solo estos tipos responden al comando de interaccion del jugador.
        public bool IsInteractive => kind == PropKind.Module || kind == PropKind.Lever || kind == PropKind.Reactor || kind == PropKind.Portal || kind == PropKind.CircuitSwitch;

        // Texto corto mostrado en pantalla cuando el jugador esta cerca.
        public string Prompt
        {
            get
            {
                if (kind == PropKind.CircuitSwitch) return Activated ? "Listo" : "E";
                if (kind == PropKind.Module) return "E";
                if (kind == PropKind.Lever) return Activated ? "Listo" : "E";
                if (kind == PropKind.Reactor) return UmbralGame.Instance.Rescued ? "Listo" : "E";
                return UmbralGame.Instance.Rescued ? "E" : "E";
            }
        }

        // Abre puertas al cumplir su requisito y anima indicadores de modulos y portal.
        void Update()
        {
            var game = UmbralGame.Instance;
            if (game == null) return;
            if (kind == PropKind.Door)
            {
                bool open = !string.IsNullOrEmpty(circuit) ? game.HasCircuit(circuit) : index == 0 ? game.Modules[0] : index == 1 ? game.Modules[1] && game.LeverActivated : game.Rescued;
                if (open && !Activated)
                {
                    Activated = true;
                    if (barrier != null) barrier.enabled = false;
                    foreach (var sprite in GetComponentsInChildren<SpriteRenderer>()) sprite.color *= new Color(1, 1, 1, .12f);
                }
            }
            if (kind == PropKind.Module && indicator != null)
            {
                indicator.transform.localPosition = new Vector3(0, Mathf.Sin(Time.time * 3 + index) * .07f, 0);
                indicator.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 2) * 7);
            }
            if (kind == PropKind.Portal && indicator != null)
                indicator.color = game.Rescued ? new Color(.3f, .95f, 1, .8f + .2f * Mathf.Sin(Time.time * 3)) : new Color(.15f, .28f, .4f, .45f);
        }

        // Ejecuta la accion propia de este objeto y actualiza el estado de la partida.
        public void Interact()
        {
            var game = UmbralGame.Instance;
            if (game.Mode != GameMode.Playing || !IsInteractive) return;
            if (kind == PropKind.CircuitSwitch)
            {
                if (Activated) return;
                Activated = true;
                game.ActivateCircuit(circuit);
                if (indicator != null) indicator.color = new Color(.3f, 1, .75f);
                game.Sound(1050, .2f, .14f);
            }
            else if (kind == PropKind.Module)
            {
                game.Modules[index] = true;
                game.Sound(1150 + 140 * index, .25f, .16f);
                game.Pulse(transform.position, new Color(.25f, 1, .85f), 2);
                gameObject.SetActive(false);
            }
            else if (kind == PropKind.Lever)
            {
                if (Activated) return;
                Activated = true;
                game.LeverActivated = true;
                if (indicator != null) indicator.color = new Color(.3f, 1, .75f);
                game.Sound(500, .2f, .12f);
            }
            else if (kind == PropKind.Reactor)
            {
                if (game.Rescued) return;
                if (game.ModuleCount < 3) return;
                game.Rescue();
            }
            else if (kind == PropKind.Portal)
            {
                if (!game.Rescued) return;
                game.Win();
            }
        }

        // Activa peligros, puntos de control y ayudas al entrar en sus zonas de contacto.
        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<UmbralPlayer>();
            if (player == null) return;
            if (kind == PropKind.Hazard) player.Respawn(true);
            else if (kind == PropKind.Checkpoint && !Activated)
            {
                Activated = true;
                player.SetCheckpoint((Vector2)transform.position + checkpointOffset);
                if (indicator != null) indicator.color = new Color(.3f, 1, .75f);
            }
            else if (kind == PropKind.Hint && !Activated)
            {
                Activated = true;
            }
        }
        // Mantiene activo el peligro mientras el jugador siga dentro del trigger.
        void OnTriggerStay2D(Collider2D other)
        {
            if (kind != PropKind.Hazard) return;
            var player = other.GetComponent<UmbralPlayer>();
            if (player != null) player.Respawn(true);
        }
    }
}

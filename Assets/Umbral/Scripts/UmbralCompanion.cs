using UnityEngine;

namespace Umbral
{
    // Seguimiento del acompañante. Si isMara es true, Mara se mueve hacia el punto de canalización;
    // si es false, el aliado sigue al jugador con un desplazamiento y suavizado.
    public sealed class UmbralCompanion : MonoBehaviour
    {
        // Cambia a true para que este aliado sea Mara, la compañera que se va al canal después del rescate.
        public bool isMara;
        public SpriteRenderer visual;
        public Sprite[] frames;
        public UmbralCharacterDefinition character;

        // Aplica los sprites y la escala del asset visual al iniciar la escena.
        void Awake()
        {
            if (character == null) return;
            frames = character.frames;
            if (visual != null && frames.Length > 0) { visual.sprite = frames[0]; visual.transform.localScale = Vector3.one * character.visualScale; }
        }
        Vector3 velocity;
        float nextSpark;

        // Lógica de movimiento principal:
        // - Aliado normal: sigue al jugador con offset y SmoothDamp.
        // - Mara: va hacia el punto de salida cuando la partida se ha rescatado.
        // El aliado normal sigue al jugador; Mara se mueve al punto de canalizacion tras el rescate.
        void Update()
        {
            var game = UmbralGame.Instance;
            if (game == null || game.player == null) return;
            if (game.Mode != GameMode.Playing) return;
            if (!isMara)
            {
                var target = game.player.transform.position + new Vector3(game.player.FacingRight ? -1 : 1, 1.1f, 0);
                target.y = Mathf.Clamp(target.y, 1, 6.9f) + Mathf.Sin(Time.time * 2.2f) * .12f;
                transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, .35f);
            }
            else if (game.Rescued)
            {
                var target = game.maraChannelPoint != null ? game.maraChannelPoint.position : transform.position;
                bool walking = Vector3.Distance(transform.position, target) > .1f;
                transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 5.5f);
                if (frames != null && frames.Length >= 20)
                    visual.sprite = frames[(walking ? 4 : 0) + Mathf.FloorToInt(Time.time * (walking ? 10 : 6)) % 4];
                if (!walking && Time.time > nextSpark)
                {
                    nextSpark = Time.time + .4f;
                    game.Pulse(transform.position + Vector3.right * .45f, new Color(.8f, .4f, 1, .5f), .5f);
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Umbral
{
    // Estados principales que determinan que pantalla y controles estan activos.
    public enum GameMode { Title, Playing, Paused, Won }

    // Control principal del juego: estado, objetivos, audio, interacción y efectos visuales.
    // Aquí se puede ajustar el volumen general, la lógica de victoria/rescate y la forma en que se
    // genera el feedback sonoro y visual del juego.
    public sealed class UmbralGame : MonoBehaviour
    {
        // Acceso global a los sistemas de partida desde los componentes del nivel.
        public static UmbralGame Instance { get; private set; }

        // Referencias asignadas en la escena; si falta alguna, revisarla en el Inspector de Unity.
        public UmbralWorldConfig world;
        public Transform maraChannelPoint;
        public Sprite fragmentSprite;
        public UmbralPlayer player;
        public UmbralCamera cameraRig;
        public Sprite effectRing;
        public Sprite[] pulseFrames;

        // Progreso y estado de partida que consultan el jugador, el HUD y los objetos del nivel.
        public GameMode Mode { get; private set; }
        public bool[] Modules { get; private set; } = new bool[3];
        public bool LeverActivated { get; set; }
        public bool Rescued { get; private set; }
        public int Deaths { get; set; }
        public int EnemiesDestroyed { get; set; }
        public float Elapsed { get; private set; }
        // Propiedades derivadas: evitan mantener contadores paralelos que podrian desincronizarse.
        public int ModuleCount => (Modules[0] ? 1 : 0) + (Modules[1] ? 1 : 0) + (Modules[2] ? 1 : 0);
        public string Sector => world != null ? world.SectorAt(player.transform.position.x) : "EXPLORACIÓN";
        public string Objective => Rescued ? "Portal" : ModuleCount == 3 ? "Módulos" :
            ModuleCount >= 2 && !HasCircuit("cryo") ? "Relé" :
            ModuleCount >= 2 && !HasCircuit("security") ? "Generador" : "Módulos";
        public UmbralProp Nearest { get; private set; }

        readonly HashSet<string> circuits = new HashSet<string>();
        UmbralDamageable[] targets;
        UmbralProp[] props;
        string notification;
        float notificationUntil;
        // Volumen global de los tonos generados proceduralmente. Cambiar aquí afecta todos los sonidos.
        float volume = .45f;
        AudioSource speaker;

        GUIStyle body, small, label, heading, title, button;
        readonly Color cyan = new Color(.35f, .94f, 1), violet = new Color(.68f, .5f, 1), orange = new Color(1, .63f, .32f);

        // Inicializa el estado global, localiza objetos de la escena y prepara audio/camara.
        void Awake()
        {
            Instance = this;
            Time.timeScale = 1;
            Mode = GameMode.Title;
            if (world == null) world = ScriptableObject.CreateInstance<UmbralWorldConfig>();
            Application.targetFrameRate = 120;
            Time.fixedDeltaTime = 1f / 60;
            targets = FindObjectsByType<UmbralDamageable>(FindObjectsSortMode.None);
            props = FindObjectsByType<UmbralProp>(FindObjectsSortMode.None);
            speaker = gameObject.AddComponent<AudioSource>();
            speaker.spatialBlend = 0;
            cameraRig.Snap();
            if (!Application.isEditor && System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-umbral-qa") >= 0)
                gameObject.AddComponent<UmbralCapture>();
        }

        void OnDestroy()
        {
            Time.timeScale = 1;
            if (Instance == this) Instance = null;
        }

        // Gestiona pausa/inicio, cronometro y la referencia al objeto interactivo mas cercano.
        void Update()
        {
            var keys = Keyboard.current;
            if (keys != null)
            {
                if (keys.enterKey.wasPressedThisFrame && Mode == GameMode.Title) StartGame();
                else if (keys.escapeKey.wasPressedThisFrame)
                {
                    if (Mode == GameMode.Playing) Pause(true);
                    else if (Mode == GameMode.Paused) Pause(false);
                }
            }
            Nearest = null;
            if (Mode != GameMode.Playing) return;
            Elapsed += Time.deltaTime;
            float closest = 1.85f;
            foreach (var prop in props)
            {
                if (prop == null || !prop.gameObject.activeInHierarchy || !prop.IsInteractive) continue;
                float distance = Vector2.Distance(prop.transform.position, player.transform.position);
                if (distance < closest) { closest = distance; Nearest = prop; }
            }
        }

        // Inicia la partida y reproduce un tono de arranque.
        public void StartGame()
        {
            Mode = GameMode.Playing;
            Time.timeScale = 1;
            Sound(660, .18f, .15f);
        }

        public bool IsGravityZone(Vector2 position)
        {
            if (props == null) return false;
            foreach (var prop in props)
                if (prop.kind == PropKind.GravityZone && prop.GetComponent<Collider2D>().OverlapPoint(position)) return true;
            return false;
        }

        // Pausa o reanuda el juego. Cambia el estado del modo y la escala de tiempo.
        public void Pause(bool paused)
        {
            Mode = paused ? GameMode.Paused : GameMode.Playing;
            Time.timeScale = paused ? 0 : 1;
        }

        // Busca el prop interactivo más cercano y ejecuta su acción.
        public void TryInteract()
        {
            if (Mode != GameMode.Playing) return;
            UmbralProp nearest = null;
            float distance = 1.85f;
            foreach (var prop in props)
            {
                if (prop == null || !prop.gameObject.activeInHierarchy || !prop.IsInteractive) continue;
                float candidate = Vector2.Distance(prop.transform.position, player.transform.position);
                if (candidate < distance) { distance = candidate; nearest = prop; }
            }
            if (nearest != null) nearest.Interact();
        }

        // Resuelve el objetivo principal cuando el jugador reúne los tres módulos.
        public void Rescue()
        {
            if (ModuleCount < 3) return;
            Rescued = true;
            Sound(1400, .5f, .18f);
            var mara = FindObjectsByType<UmbralCompanion>(FindObjectsSortMode.None);
            foreach (var ally in mara) if (ally.isMara) Pulse(ally.transform.position + Vector3.up, violet, 3);
        }

        // Finaliza la partida cuando se activa la salida o el portal.
        public void Win()
        {
            if (!Rescued) return;
            Mode = GameMode.Won;
            player.Body.linearVelocity = Vector2.zero;
            Time.timeScale = 0;
            Sound(1650, .6f, .15f);
        }

        // Se deja vacío porque el proyecto ya no usa texto de ayuda visual en pantalla.
        public void Notify(string text, float seconds)
        {
            // No hay texto de ayuda ni pistas en pantalla.
        }

        // Genera un tono sintético con frecuencia, duración y ganancia controladas.
        // Aquí se puede modificar: frecuencia (pitch), tiempo, y volumen del efecto generado.
        public void Sound(float frequency, float duration, float gain)
        {
            if (speaker == null || volume < .01f) return;

            int count = Mathf.CeilToInt(22050 * duration);
            var samples = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                float wave = Mathf.Sin(i * frequency / 22050f * Mathf.PI * 2f * (1f - t * .25f));
                samples[i] = wave * Mathf.Sin(t * Mathf.PI) * gain;
            }

            var clip = AudioClip.Create("Umbral / señal", count, 1, 22050, false);
            clip.SetData(samples, 0);
            speaker.PlayOneShot(clip, volume);
            Destroy(clip, duration + .25f);
        }

        // Efecto de destello energético. Se usa para feedback visual en objetos, rescate o puntos de interés.
        public void Pulse(Vector3 position, Color color, float size)
        {
            var obj = new GameObject("Destello de energía");
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * size;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = effectRing;
            renderer.color = color; // 
            renderer.sortingOrder = 25;
            obj.AddComponent<UmbralEffect>();
        }

        public bool HasCircuit(string circuit) => !string.IsNullOrEmpty(circuit) && circuits.Contains(circuit);

        // Registra un circuito activado; las puertas pueden consultar este estado por su nombre.
        public void ActivateCircuit(string circuit)
        {
            if (!string.IsNullOrEmpty(circuit)) circuits.Add(circuit);
        }
        // Crea el pulso central y varios fragmentos visuales para representar una explosion.
        public void Explosion(Vector3 position, Color color, float size)
        {
            Pulse(position, color, size);
            for (int i = 0; i < 8; i++)
            {
                var obj = new GameObject("Fragmento de explosión");
                obj.transform.position = position;
                obj.transform.localScale = Vector3.one * Random.Range(.06f, .13f);
                var sprite = obj.AddComponent<SpriteRenderer>();
                sprite.sprite = fragmentSprite;
                sprite.color = i % 2 == 0 ? color : Color.white;
                sprite.sortingOrder = 24;
                float angle = i * Mathf.PI / 4;
                var effect = obj.AddComponent<UmbralEffect>();
                effect.duration = .35f;
                effect.expansion = -.6f;
                effect.drift = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * Random.Range(2, 4);
            }
        }

        // Crea un proyectil para el ataque del jugador. Aquí puedes ajustar velocidad, rango y daño del disparo.
        public void Shot(Vector2 position, bool right, int gravity, UmbralMovementSettings balance, UmbralCharacterDefinition character)
        {
            var obj = new GameObject("Proyectil de energía");
            obj.transform.position = position;
            obj.transform.localScale = new Vector3(right ? 1 : -1, gravity, 1) * .85f;
            var frames = character != null && character.pulseFrames != null && character.pulseFrames.Length > 0 ? character.pulseFrames : pulseFrames;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = frames[0];
            renderer.sortingOrder = 23;
            var projectile = obj.AddComponent<UmbralProjectile>();
            projectile.frames = frames;
            projectile.speed = balance.projectileSpeed;
            projectile.range = balance.projectileRange;
            projectile.damage = balance.damage;
            projectile.right = right;
            Pulse(position, character != null ? character.energyColor : cyan, .3f);
        }

        // Crea los estilos de interfaz una sola vez para reutilizarlos en los siguientes repintados.
        void Styles()
        {
            if (body != null) return;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            body = new GUIStyle { font = font, fontSize = 22, wordWrap = true, normal = { textColor = new Color(.82f, .88f, .95f) } };
            small = new GUIStyle(body) { fontSize = 18 };
            label = new GUIStyle(body) { fontSize = 19, fontStyle = FontStyle.Bold, normal = { textColor = cyan } };
            heading = new GUIStyle(body) { fontSize = 34, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            title = new GUIStyle(heading) { fontSize = 90 };
            button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        }

        // Primitivas de dibujo reutilizadas por paneles, barras y fondos de la interfaz.
        void Box(Rect r, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        void Panel(Rect r, Color accent)
        {
            Box(r, new Color(.025f, .045f, .085f, .94f));
            Box(new Rect(r.x, r.y, 3, r.height), accent);
        }

        bool Button(Rect r, string text, bool primary = false)
        {
            GUI.backgroundColor = primary ? new Color(.13f, .55f, .65f) : new Color(.12f, .18f, .28f);
            bool result = GUI.Button(r, text, button);
            GUI.backgroundColor = Color.white;
            return result;
        }

        // Selecciona la interfaz adecuada segun el estado actual de la partida.
        void OnGUI()
        {
            Styles();
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1600f, Screen.height / 900f, 1));

            if (Mode == GameMode.Title)
            {
                DrawTitle();
                return;
            }

            if (Mode == GameMode.Paused)
            {
                DrawPause();
                return;
            }

            if (Mode == GameMode.Won)
            {
                DrawEnding();
                return;
            }

            DrawHUD();
        }

        // Muestra sector, objetivo, modulos, interaccion cercana y controles.
        void DrawHUD()
        {
            Panel(new Rect(24, 20, 430, 80), cyan);
            GUI.Label(new Rect(44, 30, 390, 25), Sector, label);
            GUI.Label(new Rect(44, 58, 390, 22), "UMBRAL", small);

            Panel(new Rect(470, 20, 735, 80), violet);
            GUI.Label(new Rect(492, 30, 690, 22), "OBJETIVO", label);
            GUI.Label(new Rect(492, 56, 690, 24), Objective, small);

            Panel(new Rect(1220, 20, 356, 80), orange);
            GUI.Label(new Rect(1240, 30, 300, 24), "MÓDULOS " + ModuleCount + "/3", label);
            for (int i = 0; i < 3; i++) Box(new Rect(1242 + i * 55, 69, 43, 7), Modules[i] ? cyan : new Color(.16f, .24f, .32f));

            if (Nearest != null)
            {
                var screen = Camera.main.WorldToScreenPoint(player.transform.position + Vector3.up * 1.25f);
                float x = Mathf.Clamp(screen.x / Screen.width * 1600 - 185, 20, 1210);
                float y = Mathf.Clamp(900 - screen.y / Screen.height * 900, 135, 700);
                Panel(new Rect(x, y, 370, 42), cyan);
                GUI.Label(new Rect(x + 12, y + 10, 350, 30), Nearest.Prompt, small);
            }

            Box(new Rect(0, 860, 1600, 40), new Color(.025f, .035f, .06f, .98f));
            GUI.Label(new Rect(30, 870, 1400, 24), "A/D MOVER   SHIFT CORRER   ESPACIO SALTAR   Q GRAVEDAD   J PULSO   E INTERACTUAR   R BALIZA   ESC PAUSA", small);
        }

        // Pantalla inicial; el boton iniciar llama a StartGame.
        void DrawTitle()
        {
            Box(new Rect(0, 0, 1600, 900), new Color(.015f, .025f, .055f, .85f));
            GUI.Label(new Rect(80, 150, 900, 120), "UMBRAL", title);

            if (Button(new Rect(84, 430, 300, 60), "INICIAR", true)) StartGame();
            if (Button(new Rect(404, 430, 180, 60), "SALIR")) Application.Quit();
        }

        // Menu de pausa con opciones de continuar, reiniciar o salir.
        void DrawPause()
        {
            Box(new Rect(0, 0, 1600, 900), new Color(.01f, .02f, .04f, .86f));
            Panel(new Rect(470, 180, 660, 440), cyan);
            GUI.Label(new Rect(510, 220, 580, 60), "PAUSA", heading);

            if (Button(new Rect(510, 380, 580, 55), "CONTINUAR", true)) Pause(false);
            if (Button(new Rect(510, 455, 580, 55), "REINICIAR")) Restart();
            if (Button(new Rect(510, 530, 580, 55), "SALIR")) Application.Quit();
        }

        // Pantalla de victoria con opciones para volver a jugar o salir.
        void DrawEnding()
        {
            Box(new Rect(0, 0, 1600, 900), new Color(.015f, .025f, .06f, .94f));
            GUI.Label(new Rect(170, 200, 1200, 80), "MISIÓN COMPLETADA", heading);

            if (Button(new Rect(170, 540, 340, 60), "VOLVER A JUGAR", true)) Restart();
            if (Button(new Rect(530, 540, 180, 60), "SALIR")) Application.Quit();
        }

        // Recarga la escena actual para reiniciar todos los objetos y el progreso.
        void Restart()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}

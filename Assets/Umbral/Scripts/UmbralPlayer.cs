using UnityEngine;
using UnityEngine.InputSystem;

namespace Umbral
{
    // Controla entrada, movimiento, salto, cambio de gravedad, ataque, animacion y reaparicion.
    // Los valores de movimiento se ajustan en UmbralMovementSettings y el aspecto en la definicion del personaje.
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public sealed class UmbralPlayer : MonoBehaviour
    {
        // Assets de configuracion; asignarlos en el Inspector permite cambiar balance y aspecto sin editar codigo.
        public UmbralMovementSettings settings;
        public UmbralCharacterDefinition character;
        public Sprite[] frames;
        public SpriteRenderer visual;

        // Estado de solo lectura usado por camara, HUD y otros componentes.
        public Rigidbody2D Body { get; private set; }
        public int GravitySign { get; private set; } = 1;
        public bool Grounded { get; private set; }
        public bool FacingRight { get; private set; } = true;
        public float Horizontal { get; private set; }
        public bool Running { get; private set; }
        public bool InGravityZone => UmbralGame.Instance != null && UmbralGame.Instance.IsGravityZone(transform.position);
        public float AttackReady => Mathf.Clamp01((Time.time - lastAttack) / (settings != null ? settings.attackCooldown : .3f));
        public Vector2 Checkpoint { get; private set; }
        public bool Invulnerable => Time.time < safeUntil;
        CapsuleCollider2D capsule;
        ContactFilter2D groundFilter;
        readonly RaycastHit2D[] hits = new RaycastHit2D[8];
        float lastFlip = -10, lastAttack = -10, safeUntil, attackUntil, hurtUntil;
        float jumpRequested = -10, lastGrounded = -10;
        float animationTime;
        int previousAnimation = -1;
        bool cutJump;
        float visualScale = 1;

        void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            if (settings == null) settings = ScriptableObject.CreateInstance<UmbralMovementSettings>();
            ApplyCharacter();
            groundFilter = new ContactFilter2D { useLayerMask = true, layerMask = 1 << 6, useTriggers = false };
            Checkpoint = transform.position;
            Body.gravityScale = settings.gravityScale;
        }

        // Lee controles y procesa acciones que no requieren el paso de fisica.
        void Update()
        {
            var game = UmbralGame.Instance;
            if (game == null) return;
            if (game.Mode != GameMode.Playing)
            {
                Horizontal = 0;
                Animate();
                return;
            }
            var keys = Keyboard.current;
            if (keys != null)
            {
                Horizontal = ((keys.dKey.isPressed || keys.rightArrowKey.isPressed) ? 1 : 0)
                    - ((keys.aKey.isPressed || keys.leftArrowKey.isPressed) ? 1 : 0);
                Running = keys.leftShiftKey.isPressed || keys.rightShiftKey.isPressed;
                if (Mathf.Abs(Horizontal) > .1f) FacingRight = Horizontal > 0;
                if (keys.spaceKey.wasPressedThisFrame) jumpRequested = Time.time;
                if (keys.spaceKey.wasReleasedThisFrame) cutJump = true;
                if (keys.qKey.wasPressedThisFrame) TryFlip();
                if (keys.jKey.isPressed) TryAttack();
                if (keys.eKey.wasPressedThisFrame) game.TryInteract();
                if (keys.rKey.wasPressedThisFrame) Respawn(false);
            }
            if (Mathf.Abs(Horizontal) > .1f) FacingRight = Horizontal > 0;
            if (transform.position.y < game.world.minSafeY || transform.position.y > game.world.maxSafeY) Respawn(true);
            Animate();
        }

        // Calcula contacto con el suelo y aplica aceleracion, salto y limite de caida.
        void FixedUpdate()
        {
            if (UmbralGame.Instance == null || UmbralGame.Instance.Mode != GameMode.Playing) return;
            Grounded = false;
            int count = capsule.Cast(Vector2.down * GravitySign, groundFilter, hits, .09f);
            for (int i = 0; i < count; i++)
                if (Body.linearVelocity.y * GravitySign <= .1f && Vector2.Dot(hits[i].normal, Vector2.up * GravitySign) > .55f) Grounded = true;
            if (Grounded) lastGrounded = Time.time;
            if (Time.time - jumpRequested < settings.jumpBuffer && Time.time - lastGrounded < settings.coyoteTime)
            {
                Body.linearVelocity = new Vector2(Body.linearVelocity.x, settings.jumpSpeed * GravitySign);
                jumpRequested = lastGrounded = -10;
                Grounded = false;
                UmbralGame.Instance.Sound(620, .07f, .12f);
            }
            if (cutJump && Body.linearVelocity.y * GravitySign > 0)
                Body.linearVelocity = new Vector2(Body.linearVelocity.x, Body.linearVelocity.y * settings.jumpReleaseMultiplier);
            cutJump = false;
            float target = Horizontal * (Running ? settings.runSpeed : settings.walkSpeed);
            float acceleration = Mathf.Abs(Horizontal) < .1f ? settings.braking : Grounded ? settings.groundAcceleration : settings.airAcceleration;
            Body.linearVelocity = new Vector2(Mathf.MoveTowards(Body.linearVelocity.x, target, acceleration * Time.fixedDeltaTime),
                Mathf.Clamp(Body.linearVelocity.y, -settings.maxFallSpeed, settings.maxFallSpeed));
        }

        // Invierte la gravedad solo dentro de una zona habilitada y respetando el tiempo de recarga.
        public bool TryFlip()
        {
            if (!InGravityZone)
            {
                return false;
            }
            if (Time.time - lastFlip < settings.flipCooldown) return false;
            lastFlip = Time.time;
            GravitySign *= -1;
            Body.gravityScale = settings.gravityScale * GravitySign;
            Body.linearVelocity = new Vector2(Body.linearVelocity.x, 0);
            Grounded = false;
            lastGrounded = jumpRequested = -10;
            visual.transform.localScale = new Vector3(visualScale, visualScale * GravitySign, 1);
            UmbralGame.Instance.Sound(GravitySign == -1 ? 850 : 420, .16f, .12f);
            UmbralGame.Instance.Pulse(transform.position, new Color(.25f, .9f, 1), 1.5f);
            return true;
        }

        // Dispara si la partida esta activa y ha terminado el enfriamiento del ataque.
        public bool TryAttack()
        {
            if (UmbralGame.Instance.Mode != GameMode.Playing || Time.time - lastAttack < settings.attackCooldown) return false;
            lastAttack = Time.time;
            attackUntil = Time.time + .22f;
            Vector2 origin = (Vector2)transform.position + new Vector2(FacingRight ? .48f : -.48f, .05f * GravitySign);
            UmbralGame.Instance.Shot(origin, FacingRight, GravitySign, settings, character);
            UmbralGame.Instance.Sound(180, .14f, .16f);
            return true;
        }

        // Guarda el punto al que volvera el jugador al morir o usar la baliza.
        public void SetCheckpoint(Vector2 position)
        {
            Checkpoint = position;
            UmbralGame.Instance.Sound(980, .13f, .12f);
        }

        // Restaura posicion y gravedad; countFailure indica si el retorno cuenta como muerte.
        public void Respawn(bool countFailure)
        {
            if (UmbralGame.Instance.Mode != GameMode.Playing || (countFailure && Invulnerable)) return;
            if (countFailure) UmbralGame.Instance.Deaths++;
            UmbralGame.Instance.Pulse(transform.position, new Color(1, .4f, .25f), 2);
            Body.position = Checkpoint;
            Body.linearVelocity = Vector2.zero;
            GravitySign = 1;
            Body.gravityScale = settings.gravityScale;
            visual.transform.localScale = new Vector3(visualScale, visualScale, 1);
            Grounded = false;
            lastGrounded = jumpRequested = -10;
            cutJump = false;
            safeUntil = Time.time + 1.2f;
            hurtUntil = Time.time + .3f;
            UmbralGame.Instance.Sound(100, .2f, .14f);
            UmbralGame.Instance.cameraRig.Snap();
        }

        // Copia sprites, escala y collider desde el asset de personaje asignado.
        [ContextMenu("Aplicar definición de personaje")]
        public void ApplyCharacter()
        {
            if (character == null) return;
            frames = character.frames;
            visualScale = character.visualScale;
            var collider = GetComponent<CapsuleCollider2D>();
            if (collider != null) collider.size = character.colliderSize;
            if (visual != null)
            {
                visual.transform.localScale = new Vector3(visualScale, visualScale * GravitySign, 1);
                if (frames != null && frames.Length > 0) visual.sprite = frames[0];
            }
        }

        // Elige la fila de animacion segun ataque, salto, movimiento o reposo.
        void Animate()
        {
            if (frames == null || frames.Length < 20 || visual == null) return;
            if (UmbralGame.Instance.Mode == GameMode.Title) { visual.sprite = frames[0]; return; }
            int row;
            float fps;
            if (Time.time < attackUntil) { row = 4; fps = 12; }
            else if (!Grounded) { row = 3; fps = 8; }
            else if (Mathf.Abs(Horizontal) < .1f) { row = 0; fps = 6; }
            else { row = Running ? 2 : 1; fps = Running ? 14 : 10; }
            if (row != previousAnimation) { animationTime = 0; previousAnimation = row; }
            animationTime += Time.deltaTime;
            int index = Mathf.FloorToInt(animationTime * fps) % 4;
            if (row == 3) index = Body != null && Body.linearVelocity.y * GravitySign > .5f ? 1 : (Grounded ? 3 : 2);
            if (row == 4) index = Mathf.Clamp(Mathf.FloorToInt((Time.time - lastAttack) * 12), 0, 3);
            visual.sprite = frames[row * 4 + index];
            visual.flipX = !FacingRight;
            visual.color = Time.time < hurtUntil ? new Color(1, .45f, .35f) :
                Invulnerable && Mathf.FloorToInt(Time.time * 12) % 2 == 0 ? new Color(1, 1, 1, .35f) : Color.white;
        }
    }
}

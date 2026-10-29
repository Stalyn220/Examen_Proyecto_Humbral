using UnityEngine;
namespace Umbral
{
    // Enemigo que patrulla entre dos puntos, puede recibir dano y elimina al jugador por contacto.
    [RequireComponent(typeof(UmbralDamageable))]
    public sealed class UmbralDrone : MonoBehaviour
    {
        // Puntos de patrulla en coordenadas del mundo; velocidad se mide en unidades por segundo.
        public Vector2 left, right;
        public SpriteRenderer eye;
        public float speed = 1.35f;
        public bool Disabled => damageable == null || damageable.Destroyed;
        bool goRight;
        UmbralDamageable damageable;
        void Awake() { damageable = GetComponent<UmbralDamageable>(); }
        // Avanza hacia el extremo actual y cambia de sentido al llegar.
        void Update()
        {
            if (Disabled || UmbralGame.Instance == null || UmbralGame.Instance.Mode != GameMode.Playing) return;
            var target = goRight ? right : left;
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if (Vector2.Distance(transform.position, target) < .03f) goRight = !goRight;
        }
        // Fuerza la destruccion del dron usando el sistema de salud compartido.
        public void Disable() { if (!Disabled) damageable.Hit(damageable.Health); }

        // El contacto con un dron activo hace que el jugador reaparezca y cuente un fallo.
        void OnTriggerStay2D(Collider2D other)
        {
            var player = other.GetComponent<UmbralPlayer>();
            if (!Disabled && player != null) player.Respawn(true);
        }
    }
}

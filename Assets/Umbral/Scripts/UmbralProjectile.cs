using UnityEngine;

namespace Umbral
{
    // Proyectil del ataque del jugador: avanza, detecta colisiones en su trayectoria
    // y desaparece al impactar o alcanzar el alcance configurado.
    public sealed class UmbralProjectile : MonoBehaviour
    {
        // Ajustar estos valores cambia la velocidad, alcance y dano de este proyectil.
        public float speed = 18, range = 7;
        public int damage = 1;
        public bool right = true;
        // Sprites opcionales para animar el pulso durante su vuelo.
        public Sprite[] frames;
        float travelled, age;
        SpriteRenderer visual;
        readonly RaycastHit2D[] hits = new RaycastHit2D[16];
        ContactFilter2D filter;
        void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            // Solo consulta las capas configuradas para objetivos y elementos impactables.
            filter = new ContactFilter2D { useLayerMask = true, layerMask = (1 << 6) | (1 << 7), useTriggers = true };
        }

        // Se mueve en FixedUpdate y usa CircleCast para no atravesar objetos entre fotogramas.
        void FixedUpdate()
        {
            if (UmbralGame.Instance == null || UmbralGame.Instance.Mode != GameMode.Playing) return;
            float distance = Mathf.Min(speed * Time.fixedDeltaTime, range - travelled);
            Vector2 direction = right ? Vector2.right : Vector2.left;
            int count = Physics2D.CircleCast(transform.position, .11f, direction, filter, hits, distance);
            RaycastHit2D closest = default;
            float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].distance < best) { best = hits[i].distance; closest = hits[i]; }
            }
            if (closest.collider != null)
            {
                var target = closest.collider.GetComponentInParent<UmbralDamageable>();
                if (target != null) target.Hit(damage);
                else UmbralGame.Instance.Pulse(closest.point, new Color(.35f, .9f, 1), .4f);
                Destroy(gameObject);
                return;
            }
            transform.position += (Vector3)direction * distance;
            travelled += distance;
            if (travelled >= range) Destroy(gameObject);
        }
        // Cambia el sprite segun el tiempo transcurrido para dar movimiento al efecto.
        void Update()
        {
            age += Time.deltaTime;
            if (frames != null && frames.Length > 0)
                visual.sprite = frames[Mathf.Min(frames.Length - 1, 1 + Mathf.FloorToInt(age * 18) % Mathf.Max(1, frames.Length - 1))];
        }
    }
}

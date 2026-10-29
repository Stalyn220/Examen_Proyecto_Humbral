using UnityEngine;

namespace Umbral
{
    // Salud compartida por enemigos y blancos destructibles; al agotarse desactiva colisiones,
    // actualiza el progreso y genera efectos de destruccion.
    public sealed class UmbralDamageable : MonoBehaviour
    {
        // Ajustes de resistencia, conteo de enemigos, circuito que se activa y aspecto de la explosion.
        [Min(1)] public int maxHealth = 1;
        public bool countsAsEnemy = true;
        public string circuitOnDestroy;
        public string targetName = "Centinela";
        public Color explosionColor = new Color(1, .45f, .18f);
        public int Health { get; private set; }
        public bool Destroyed => Health <= 0;
        SpriteRenderer[] visuals;
        Color[] initial;
        float flashUntil;

        // Guarda colores originales para poder restaurarlos despues del destello de impacto.
        void Awake()
        {
            Health = maxHealth;
            visuals = GetComponentsInChildren<SpriteRenderer>();
            initial = new Color[visuals.Length];
            for (int i = 0; i < visuals.Length; i++) initial[i] = visuals[i].color;
        }
        void Update()
        {
            if (Destroyed) return;
            for (int i = 0; i < visuals.Length; i++)
                if (visuals[i] != null) visuals[i].color = Time.time < flashUntil ? Color.white : initial[i];
        }
        // Resta salud; al llegar a cero desactiva el objetivo y comunica la destruccion al juego.
        public void Hit(int damage)
        {
            if (Destroyed || damage <= 0) return;
            Health = Mathf.Max(0, Health - damage);
            flashUntil = Time.time + .1f;
            var game = UmbralGame.Instance;
            if (!Destroyed)
            {
                game.Pulse(transform.position, new Color(1, .65f, .3f), .7f);
                game.Sound(220, .06f, .13f);
                return;
            }
            foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
            foreach (var renderer in visuals) renderer.enabled = false;
            if (countsAsEnemy) game.EnemiesDestroyed++;
            if (!string.IsNullOrEmpty(circuitOnDestroy)) game.ActivateCircuit(circuitOnDestroy);
            game.Explosion(transform.position, explosionColor, 1.3f);
            game.Sound(85, .16f, .17f);
            Destroy(gameObject, .2f);
        }
    }
}

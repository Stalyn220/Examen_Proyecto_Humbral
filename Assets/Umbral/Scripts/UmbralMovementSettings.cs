using UnityEngine;

namespace Umbral
{
    // Valores de balance reutilizables para controlar desplazamiento, salto, gravedad y disparo.
    // Cambiar estos campos en el asset Balance ajusta al jugador sin tocar UmbralPlayer.
    [CreateAssetMenu(menuName = "Umbral/Ajustes de movimiento y combate", fileName = "Balance")]
    public sealed class UmbralMovementSettings : ScriptableObject
    {
        // Velocidades maximas y rapidez con la que el jugador las alcanza o frena.
        [Header("Desplazamiento · unidades por segundo")]
        [Min(.1f)] public float walkSpeed = 4.8f;
        [Min(.1f)] public float runSpeed = 7.2f;
        [Min(1)] public float groundAcceleration = 70;
        [Min(1)] public float braking = 85;
        [Min(1)] public float airAcceleration = 45;
        // Ventanas de tolerancia del salto y tiempo de espera entre inversiones de gravedad.
        [Header("Salto y gravedad")]
        [Min(1)] public float jumpSpeed = 8.4f;
        [Min(.1f)] public float gravityScale = 2.1f;
        [Min(1)] public float maxFallSpeed = 16;
        [Range(.02f, .3f)] public float jumpBuffer = .16f;
        [Range(0, .3f)] public float coyoteTime = .12f;
        [Range(.2f, 1)] public float jumpReleaseMultiplier = .55f;
        [Min(.05f)] public float flipCooldown = .3f;
        // Tiempo entre ataques, velocidad y alcance del proyectil, y dano por impacto.
        [Header("Disparo")]
        [Min(.05f)] public float attackCooldown = .3f;
        [Min(1)] public float projectileSpeed = 18;
        [Min(.5f)] public float projectileRange = 7;
        [Min(1)] public int damage = 1;
    }
}

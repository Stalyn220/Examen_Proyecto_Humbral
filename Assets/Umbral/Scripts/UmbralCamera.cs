using UnityEngine;
namespace Umbral
{
    // Camara ortografica que sigue al jugador, anticipa su direccion y respeta los limites del mundo.
    public sealed class UmbralCamera : MonoBehaviour
    {
        public Transform target;
        // smoothTime controla el suavizado; lookAhead cuanto se desplaza el encuadre hacia delante.
        [Min(.03f)] public float smoothTime = .14f;
        public float lookAhead = 2.6f;
        Vector3 velocity;
        Camera view;
        void Awake() { view = GetComponent<Camera>(); }
        // Coloca la camara inmediatamente en su posicion objetivo, por ejemplo tras reaparecer.
        public void Snap() { velocity = Vector3.zero; transform.position = Position(); }

        // Calcula el encuadre deseado usando los limites configurados y la direccion del jugador.
        Vector3 Position()
        {
            if (view == null) view = GetComponent<Camera>();
            var world = UmbralGame.Instance != null ? UmbralGame.Instance.world : null;
            float min = world != null ? world.minX : -4, max = world != null ? world.maxX : 222;
            float half = view.orthographicSize * view.aspect;
            float lead = target.TryGetComponent<UmbralPlayer>(out var player) ? (player.FacingRight ? lookAhead : -lookAhead) : lookAhead;
            float x = Mathf.Clamp(target.position.x + lead, min + half, Mathf.Max(min + half, max - half));
            return new Vector3(x, world != null ? world.cameraY : 4, -10);
        }
        // Se ejecuta despues del movimiento para evitar que la camara persiga una posicion desactualizada.
        void LateUpdate()
        {
            if (target != null) transform.position = Vector3.SmoothDamp(transform.position, Position(), ref velocity, smoothTime);
        }
    }
}

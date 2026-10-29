using UnityEngine;

namespace Umbral
{
    // Efecto temporal para destellos y fragmentos: crece o se encoge, se desplaza,
    // se desvanece y se destruye al terminar su duracion.
    public sealed class UmbralEffect : MonoBehaviour
    {
        // duration controla cuanto vive; expansion define el cambio de escala durante ese tiempo.
        public float duration = .4f, expansion = 2;
        // Desplazamiento aplicado por segundo; dejarlo en cero mantiene el efecto en su sitio.
        public Vector3 drift;
        // Secuencia opcional de sprites para animar el efecto.
        public Sprite[] frames;
        float age;
        SpriteRenderer visual;
        Color initial;
        Vector3 originalScale;
        void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            initial = visual.color;
            originalScale = transform.localScale;
        }
        // Actualiza interpolaciones visuales y elimina el objeto al terminar.
        void Update()
        {
            age += Time.deltaTime;
            float t = age / duration;
            transform.position += drift * Time.deltaTime;
            transform.localScale = originalScale * (1 + expansion * t);
            visual.color = initial * new Color(1, 1, 1, 1 - t);
            if (frames != null && frames.Length > 0) visual.sprite = frames[Mathf.Min(frames.Length - 1, (int)(t * frames.Length))];
            if (t >= 1) Destroy(gameObject);
        }
    }
}

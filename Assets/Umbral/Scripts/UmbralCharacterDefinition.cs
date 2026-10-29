using UnityEngine;

namespace Umbral
{
    // Asset visual de personaje compartido por el jugador y sus aliados.
    // Permite cambiar sprites, tamano del collider, escala y color de energia desde Unity.
    [CreateAssetMenu(menuName = "Umbral/Definición de personaje", fileName = "NuevoPersonaje")]
    public sealed class UmbralCharacterDefinition : ScriptableObject
    {
        public string displayName = "Leo";
        [Tooltip("20 sprites: Idle 0-3, Walk 4-7, Run 8-11, Jump 12-15, Attack 16-19.")]
        public Sprite[] frames = new Sprite[20];
        [Tooltip("Cuatro fases del pulso; si queda vacío se usa el pulso de la misión.")]
        public Sprite[] pulseFrames;
        // Color aplicado a los efectos de energia asociados a este personaje.
        public Color energyColor = new Color(.2f, .9f, 1);
        // Ajusta la forma del collider de juego; visualScale cambia solo el tamano del sprite.
        public Vector2 colliderSize = new Vector2(.63f, 1.48f);
        [Range(.25f, 3)] public float visualScale = 1;
    }
}

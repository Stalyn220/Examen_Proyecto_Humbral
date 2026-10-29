using UnityEngine;

namespace Umbral
{
    // Datos globales del recorrido: limites, altura de camara, zonas seguras y nombres de sectores.
    // Crear este asset permite ajustar el mundo desde el Inspector sin cambiar la logica del juego.
    [CreateAssetMenu(menuName = "Umbral/Configuración del mundo", fileName = "Mundo")]
    public sealed class UmbralWorldConfig : ScriptableObject
    {
        // Sector mostrado en el HUD; startX indica donde empieza dentro del nivel.
        [System.Serializable]
        public struct SectorDefinition
        {
            public string title;
            public float startX;
            public SectorDefinition(string title, float startX) { this.title = title; this.startX = startX; }
        }

        // Limites horizontales de la camara y limites verticales que hacen reaparecer al jugador.
        public float minX = -4;
        public float maxX = 222;
        public float cameraY = 4;
        public float minSafeY = -4;
        public float maxSafeY = 13;

        // Cada entrada debe estar ordenada por startX para que SectorAt encuentre el sector correcto.
        public SectorDefinition[] sectors = new[] {
            new SectorDefinition("DESPERTAR", 0), new SectorDefinition("CONDUCTOS", 32),
            new SectorDefinition("LABORATORIO CRIOGÉNICO", 68), new SectorDefinition("NÚCLEO DE SEGURIDAD", 108),
            new SectorDefinition("REACTOR", 146), new SectorDefinition("HANGAR DE EVACUACIÓN", 184)
        };

        // Devuelve el nombre y numero del sector que contiene la coordenada horizontal indicada.
        public string SectorAt(float x)
        {
            for (int i = sectors.Length - 1; i >= 0; i--)
                if (x >= sectors[i].startX) return (i + 1).ToString("00") + " / " + sectors[i].title;
            return sectors.Length > 0 ? "01 / " + sectors[0].title : "EXPLORACIÓN";
        }
    }
}

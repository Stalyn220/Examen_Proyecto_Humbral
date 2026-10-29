using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Umbral;

namespace UmbralEditor
{
    public sealed class UmbralRoomWindow : EditorWindow
    {
        string roomName = "SECTOR EXPERIMENTAL";
        int width = 32;
        bool gravityZone = true, movePortal = true;
        [MenuItem("Umbral/Crear sala adicional...")]
        static void Open() => GetWindow<UmbralRoomWindow>("Nueva sala Umbral");
        void OnGUI()
        {
            EditorGUILayout.LabelField("Añadir un sector conectado", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Abre la escena Umbral y sal de Play. La herramienta añade una sala plana, conecta su acceso y amplía los límites de cámara. Después puedes dibujar obstáculos en su Tilemap.", MessageType.Info);
            roomName = EditorGUILayout.TextField("Nombre del sector", roomName);
            width = EditorGUILayout.IntSlider("Anchura en unidades", width, 20, 80);
            gravityZone = EditorGUILayout.Toggle("Incluir zona de gravedad", gravityZone);
            movePortal = EditorGUILayout.Toggle("Mover portal al nuevo final", movePortal);
            GUI.enabled = !EditorApplication.isPlaying;
            if (GUILayout.Button("Crear sala en la escena actual"))
            {
                UmbralRoomAuthoring.AddRoom(roomName, width, gravityZone, movePortal);
                Close();
            }
            GUI.enabled = true;
        }
    }

    public static class UmbralRoomAuthoring
    {
        public static GameObject AddRoom(string name, int width, bool gravityZone, bool movePortal)
        {
            var game = Object.FindFirstObjectByType<UmbralGame>();
            if (game == null || game.world == null) throw new System.InvalidOperationException("Abre primero Assets/Umbral/Scenes/Umbral.unity.");
            var tile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Umbral/Generated/MetalSurfaceTile.asset");
            var solid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Umbral/Generated/SolidSprite.asset");
            int start = Mathf.RoundToInt(game.world.maxX), end = start + Mathf.Clamp(width, 20, 80);
            var room = new GameObject("Sector adicional · " + name);
            Undo.RegisterCreatedObjectUndo(room, "Crear sala Umbral");
            foreach (var old in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                if (!old.HasTile(new Vector3Int(start - 1, 1, 0))) continue;
                Undo.RecordObject(old, "Abrir acceso a la sala");
                for (int y = 0; y < 8; y++) old.SetTile(new Vector3Int(start - 1, y, 0), null);
            }
            var grid = new GameObject("Grid"); grid.transform.SetParent(room.transform); grid.AddComponent<Grid>();
            var geometry = new GameObject("Tilemap · geometría editable"); geometry.transform.SetParent(grid.transform); geometry.layer = 6;
            var map = geometry.AddComponent<Tilemap>();
            geometry.AddComponent<TilemapRenderer>().sortingOrder = 5;
            geometry.AddComponent<TilemapCollider2D>().sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Umbral/Generated/SurfacePhysics.asset");
            for (int x = start; x < end; x++) { map.SetTile(new Vector3Int(x, -1, 0), tile); map.SetTile(new Vector3Int(x, 8, 0), tile); }
            for (int y = 0; y < 8; y++) map.SetTile(new Vector3Int(end - 1, y, 0), tile);
            var backdrop = Draw(room.transform, solid, "Fondo de nueva sala", new Vector2((start + end) * .5f, 4), new Vector2(width, 8), new Color(.035f, .07f, .12f), -20);
            for (int x = start + 2; x < end; x += 5)
            {
                Draw(room.transform, solid, "Panel vertical", new Vector2(x, 4), new Vector2(.08f, 8), new Color(.13f, .2f, .27f), -10);
                Draw(room.transform, solid, "Luz cian", new Vector2(x + 1, 6.5f), new Vector2(1.6f, .08f), Color.cyan, -9);
            }
            var labelObject = new GameObject("Nombre del sector"); labelObject.transform.SetParent(room.transform);
            labelObject.transform.position = new Vector3(start + 9, 4, 0);
            var label = labelObject.AddComponent<TextMesh>(); label.text = name; label.fontSize = 60; label.characterSize = .08f; label.anchor = TextAnchor.MiddleCenter; label.color = new Color(.4f, .6f, .7f);
            label.GetComponent<MeshRenderer>().sortingOrder = -5;
            var beacon = new GameObject("Baliza de nueva sala"); beacon.transform.SetParent(room.transform); beacon.transform.position = new Vector3(start + 2, 1, 0);
            var checkpoint = beacon.AddComponent<UmbralProp>(); checkpoint.kind = PropKind.Checkpoint;
            var beaconCollider = beacon.AddComponent<BoxCollider2D>(); beaconCollider.isTrigger = true; beaconCollider.size = new Vector2(1, 2);
            checkpoint.indicator = Draw(beacon.transform, solid, "Luz de baliza", new Vector2(start + 2, .45f), new Vector2(.2f, .65f), Color.cyan, 7);
            if (gravityZone)
            {
                var zone = new GameObject("Zona Q"); zone.transform.SetParent(room.transform); zone.transform.position = new Vector3((start + end) * .5f, 4, 0);
                zone.AddComponent<UmbralProp>().kind = PropKind.GravityZone;
                var collider = zone.AddComponent<BoxCollider2D>(); collider.isTrigger = true; collider.size = new Vector2(width - 4, 8);
                foreach (var x in new[] { start + 2, end - 2 }) Draw(zone.transform, solid, "Límite de gravedad", new Vector2(x, 4), new Vector2(.05f, 8), new Color(.6f, .4f, 1, .6f), 0);
            }
            Undo.RecordObject(game.world, "Ampliar configuración del mundo");
            game.world.maxX = end;
            game.world.sectors = game.world.sectors.Concat(new[] { new UmbralWorldConfig.SectorDefinition(name, start) }).ToArray();
            EditorUtility.SetDirty(game.world);
            if (movePortal)
            {
                var portal = Object.FindObjectsByType<UmbralProp>(FindObjectsSortMode.None).FirstOrDefault(p => p.kind == PropKind.Portal);
                if (portal != null) { Undo.RecordObject(portal.transform, "Mover portal"); portal.transform.position = new Vector3(end - 8, portal.transform.position.y, 0); }
            }
            EditorSceneManager.MarkSceneDirty(room.scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = room;
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log("Sala añadida. Guarda la escena con Ctrl+S y prueba la conexión.");
            return room;
        }
        static SpriteRenderer Draw(Transform parent, Sprite sprite, string name, Vector2 position, Vector2 size, Color color, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = position; go.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order;
            return renderer;
        }
    }

    public sealed class UmbralCharacterWindow : EditorWindow
    {
        Texture2D atlas;
        string characterName = "Explorador";
        float ppu = 132;
        [MenuItem("Umbral/Importar personaje desde atlas 4x5...")]
        static void Open() => GetWindow<UmbralCharacterWindow>("Importar personaje");
        void OnGUI()
        {
            EditorGUILayout.LabelField("Personaje con veinte cuadros", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Arrastra un PNG importado dentro de Assets. Filas: Idle, Walk, Run, Jump, Attack; cuatro columnas. El original se conserva y se generan sprites y una definición de personaje.", MessageType.Info);
            atlas = (Texture2D)EditorGUILayout.ObjectField("Atlas PNG", atlas, typeof(Texture2D), false);
            characterName = EditorGUILayout.TextField("Nombre", characterName);
            ppu = EditorGUILayout.FloatField("Pixels Per Unit", ppu);
            GUI.enabled = atlas != null && ppu > 0 && !EditorApplication.isPlaying;
            if (GUILayout.Button("Crear definición de personaje"))
            {
                string prefix = "Custom_" + Regex.Replace(characterName, @"[^a-zA-Z0-9_]", "_");
                string path = "Assets/Umbral/Config/" + prefix + ".asset";
                if (AssetDatabase.LoadMainAssetAtPath(path) != null && !EditorUtility.DisplayDialog("Reemplazar personaje", "Ya existe una definición con ese nombre. Sus sprites se volverán a crear.", "Reemplazar", "Cancelar")) return;
                var frames = UmbralBuilder.SliceCharacterAtlas(AssetDatabase.GetAssetPath(atlas), prefix, ppu);
                var definition = UmbralBuilder.CharacterDefinition(prefix, frames, null, Color.cyan);
                definition.displayName = characterName;
                EditorUtility.SetDirty(definition);
                AssetDatabase.SaveAssets();
                Selection.activeObject = definition;
                EditorGUIUtility.PingObject(definition);
                Close();
            }
            GUI.enabled = true;
        }
    }
}

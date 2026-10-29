using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Umbral;

namespace UmbralEditor
{
    public static class UmbralBuilder
    {
        const string Root = "Assets/Umbral";
        public const string ScenePath = Root + "/Scenes/Umbral.unity";
        static Sprite white, ring, module, droneSprite, nexoSprite, capsuleSprite, spikeSprite, panelSprite, pipeSprite;
        static Sprite floorSprite;
        static Tile floorTile;
        static Tilemap collisionMap;
        static Transform scenery, gameplay;
        static Sprite[] leo, mara, pulses;
        static UmbralMovementSettings balance;
        static UmbralWorldConfig world;
        static UmbralCharacterDefinition leoDefinition, maraDefinition;
        static readonly Color Cyan = new Color(.22f, .86f, 1), Violet = new Color(.65f, .39f, 1), Orange = new Color(1, .55f, .2f);

        [MenuItem("Umbral/Generar o reconstruir prototipo")]
        public static void Generate()
        {
            foreach (var path in new[] { "/Generated", "/Scenes", "/Prefabs", "/Config" }) Directory.CreateDirectory(Root + path);
            AssetDatabase.Refresh();
            leo = ImportAtlas("Leo_Atlas_4x5", 5, true);
            mara = ImportAtlas("Mara_Atlas_4x5", 5, true);
            pulses = ImportAtlas("Efectos_Atlas_4x2", 2, false);
            balance = AssetDatabase.LoadAssetAtPath<UmbralMovementSettings>(Root + "/Config/Balance.asset");
            if (balance == null) { balance = ScriptableObject.CreateInstance<UmbralMovementSettings>(); SaveAsset(balance, Root + "/Config/Balance.asset"); }
            world = AssetDatabase.LoadAssetAtPath<UmbralWorldConfig>(Root + "/Config/Mundo.asset");
            if (world == null) { world = ScriptableObject.CreateInstance<UmbralWorldConfig>(); SaveAsset(world, Root + "/Config/Mundo.asset"); }
            var defaults = ScriptableObject.CreateInstance<UmbralWorldConfig>();
            world.minX = defaults.minX; world.maxX = defaults.maxX; world.sectors = defaults.sectors;
            UnityEngine.Object.DestroyImmediate(defaults);
            EditorUtility.SetDirty(world);
            leoDefinition = CharacterDefinition("Leo", leo, pulses.Take(4).ToArray(), Cyan);
            maraDefinition = CharacterDefinition("Mara", mara, pulses.Skip(4).Take(4).ToArray(), Violet);
            white = Texture("Solid", 2, 2, (x, y) => Color.white, 2);
            ring = Texture("EnergyRing", 64, 64, (x, y) =>
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                return distance > 26 && distance < 30 ? Color.white : distance < 26 ? new Color(1, 1, 1, .025f) : Color.clear;
            }, 64);
            floorSprite = Texture("MetalTile", 32, 32, (x, y) =>
            {
                if (y >= 29) return y == 31 ? new Color(.5f, .66f, .76f) : new Color(.26f, .37f, .46f);
                if (y < 3 || x == 0 || x == 31) return new Color(.055f, .085f, .13f);
                if ((x == 4 || x == 27) && (y == 7 || y == 25)) return new Color(.46f, .57f, .64f);
                if (y == 16 && x > 6 && x < 25) return new Color(.11f, .16f, .23f);
                return ((x / 3 + y / 3) % 7 == 0) ? new Color(.20f, .27f, .34f) : new Color(.17f, .23f, .30f);
            }, 32);
            panelSprite = Texture("WallPanel", 32, 32, (x, y) =>
            {
                if (x < 2 || y < 2) return new Color(.025f, .04f, .075f);
                if (x == 2 || y == 30) return new Color(.085f, .13f, .19f);
                if (x > 9 && x < 24 && y > 10 && y < 24) return new Color(.04f, .065f, .11f);
                if (x == 5 && y == 26) return new Color(.16f, .23f, .29f);
                return new Color(.055f, .085f, .135f);
            }, 32);
            pipeSprite = Texture("Pipe", 16, 16, (x, y) =>
                x > 4 && x < 12 ? new Color(.095f + (x == 6 ? .08f : 0), .145f, .21f) : Color.clear, 16);
            spikeSprite = Texture("Spikes", 32, 32, (x, y) =>
            {
                int dx = Mathf.Abs(x % 16 - 8);
                if (y < 3) return new Color(.24f, .16f, .18f);
                if (y < 26 - dx * 3) return x % 16 < 8 ? new Color(1, .39f, .12f) : new Color(.65f, .16f, .1f);
                return Color.clear;
            }, 32);
            module = Texture("EnergyModule", 32, 32, (x, y) =>
            {
                if (x < 7 || x > 24 || y < 3 || y > 28) return Color.clear;
                if (y < 7 || y > 24) return new Color(.44f, .59f, .68f);
                if (x < 10 || x > 21) return new Color(.12f, .23f, .30f);
                return x < 14 ? new Color(.64f, 1, 1) : Cyan;
            }, 32);
            droneSprite = PixelSprite("Centinela", new[] {
                "................", "...GGGGGGGG.....", "..GWWWWWWWWG....", "GGWWWWWWWWWWGG..", "GWWDDDDDDDDWWG..", "GGWDRRRRRRDWGG..", "..GDDDRRDDDG....", "...GGGGGGGG.....", "......OO........", "......o........." }, 14);
            nexoSprite = PixelSprite("Nexo", new[] {
                ".......O........", ".......G........", "...WWWWWWWW.....", "..WWWWWWWWWW....", "..WDDDDDDDDW....", "..WDDCDDCDDW....", "..WDDCDDCDDW....", "..WWWWWWWWWW....", "...GGGGGGGG.....", "......CC........", ".......c........" }, 17);
            capsuleSprite = PixelSprite("Capsule", new[] {
                "...GGGGGGGG...", "..GWWWWWWWWG..", ".GWGGGGGGGGWG.", ".WGDCCCCCCD GW.", ".WGDccccccDGW.", ".WGDccccccDGW.", ".WGDccccccDGW.", ".WGDcccccCDGW.", ".WGDccccCCDGW.", ".WGDcccCCCDGW.", ".WGDccCCCCDGW.", ".WGDccccccDGW.", ".WGDccccccDGW.", ".WGDCCCCCCDGW.", ".GWGGGGGGGGWG.", "..GWWWWWWWWG..", "...GGGGGGGG..." }, 6);

            floorTile = ScriptableObject.CreateInstance<Tile>();
            floorTile.sprite = floorSprite;
            floorTile.colliderType = Tile.ColliderType.Grid;
            SaveAsset(floorTile, Root + "/Generated/MetalSurfaceTile.asset");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scenery = new GameObject("01 · Estación / fondo y maquinaria").transform;
            gameplay = new GameObject("02 · Recorrido / objetivos y peligros").transform;
            var grid = new GameObject("Laboratorio · Tilemap de superficies");
            grid.AddComponent<Grid>();
            var tiles = new GameObject("Suelo, plataformas y techo");
            tiles.transform.SetParent(grid.transform);
            tiles.layer = 6;
            collisionMap = tiles.AddComponent<Tilemap>();
            var tileRenderer = tiles.AddComponent<TilemapRenderer>();
            tileRenderer.sortingOrder = 5;
            tiles.AddComponent<TilemapCollider2D>();
            var physicsMaterial = new PhysicsMaterial2D("Superficies sin fricción") { friction = 0, bounciness = 0 };
            SaveAsset(physicsMaterial, Root + "/Generated/SurfacePhysics.asset");
            tiles.GetComponent<TilemapCollider2D>().sharedMaterial = physicsMaterial;

            Background();
            Layout();
            var player = CreatePlayer(physicsMaterial);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.backgroundColor = new Color(.018f, .025f, .055f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            var rig = cameraObject.AddComponent<UmbralCamera>();
            rig.target = player.transform;
            var game = new GameObject("UMBRAL · Control de misión").AddComponent<UmbralGame>();
            game.world = world;
            game.fragmentSprite = white;
            game.maraChannelPoint = GameObject.Find("Mara · Punto de canalización").transform;
            game.player = player;
            game.cameraRig = rig;
            game.effectRing = ring;
            game.pulseFrames = pulses.Take(4).ToArray();
            Companion(false, new Vector2(1, 2), nexoSprite);
            Companion(true, new Vector2(179.5f, 1), mara[0]);
            cameraObject.transform.position = new Vector3(7, 4, -10);

            PlayerSettings.companyName = "Stalyn Quispe";
            PlayerSettings.productName = "Proyecto Umbral";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = true;
            Application.targetFrameRate = 120;
            Time.fixedDeltaTime = 1f / 60;
            QualitySettings.vSyncCount = 1;
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            tags.FindProperty("layers").GetArrayElementAtIndex(6).stringValue = "UmbralSolid";
            tags.FindProperty("layers").GetArrayElementAtIndex(7).stringValue = "UmbralDrone";
            tags.ApplyModifiedPropertiesWithoutUndo();
            Physics2D.IgnoreLayerCollision(7, 7);
            RenderSettings.ambientLight = Color.white;
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log("UMBRAL_GENERATED_OK · " + ScenePath);
        }

        static Sprite[] ImportAtlas(string filename, int rows, bool character)
        {
            return ImportAtlasFromPath(Root + "/Art/" + filename + ".png", filename, rows, character, character ? 132 : 360);
        }
        public static Sprite[] SliceCharacterAtlas(string assetPath, string prefix, float pixelsPerUnit)
            => ImportAtlasFromPath(assetPath, prefix, 5, true, pixelsPerUnit);
        static Sprite[] ImportAtlasFromPath(string path, string filename, int rows, bool character, float pixelsPerUnit)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Sprite[] sprites = new Sprite[rows * 4];
            for (int row = 0; row < rows; row++)
            for (int col = 0; col < 4; col++)
            {
                int x0 = Mathf.RoundToInt(col * texture.width / 4f), x1 = Mathf.RoundToInt((col + 1) * texture.width / 4f);
                int y0 = Mathf.RoundToInt((rows - row - 1) * texture.height / (float)rows), y1 = Mathf.RoundToInt((rows - row) * texture.height / (float)rows);
                Vector2 pivot = new Vector2(.5f, .5f);
                if (character)
                {
                    int minY = y1, maxY = y0;
                    for (int y = y0; y < y1; y++) for (int x = x0; x < x1; x++)
                        if (texture.GetPixel(x, y).a > .8f) { minY = Mathf.Min(y, minY); maxY = Mathf.Max(y, maxY); }
                    int minX = x1, maxX = x0;
                    for (int y = minY + (maxY - minY) / 2; y <= maxY; y++) for (int x = x0; x < x1; x++)
                        if (texture.GetPixel(x, y).a > .8f) { minX = Mathf.Min(x, minX); maxX = Mathf.Max(x, maxX); }
                    pivot = new Vector2(((minX + maxX) * .5f - x0) / (x1 - x0), ((minY + maxY) * .5f - y0) / (y1 - y0));
                }
                var sprite = Sprite.Create(texture, new Rect(x0, y0, x1 - x0, y1 - y0), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                sprite.name = filename + "_" + row + "_" + col;
                SaveAsset(sprite, Root + "/Generated/" + sprite.name + ".asset");
                sprites[row * 4 + col] = sprite;
            }
            return sprites;
        }

        public static UmbralCharacterDefinition CharacterDefinition(string name, Sprite[] frames, Sprite[] effects, Color energy)
        {
            string path = Root + "/Config/" + name + ".asset";
            var definition = AssetDatabase.LoadAssetAtPath<UmbralCharacterDefinition>(path);
            if (definition == null) { definition = ScriptableObject.CreateInstance<UmbralCharacterDefinition>(); SaveAsset(definition, path); }
            definition.displayName = name; definition.frames = frames; definition.pulseFrames = effects; definition.energyColor = energy;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        static void SaveAsset(UnityEngine.Object asset, string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
        }
        static Sprite Texture(string name, int width, int height, Func<int, int, Color> pixel, float ppu)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Point };
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) texture.SetPixel(x, y, pixel(x, y));
            texture.Apply();
            SaveAsset(texture, Root + "/Generated/" + name + ".asset");
            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            SaveAsset(sprite, Root + "/Generated/" + name + "Sprite.asset");
            return sprite;
        }
        static Sprite PixelSprite(string name, string[] pixels, float ppu)
        {
            return Texture(name, pixels.Max(p => p.Length), pixels.Length, (x, y) =>
            {
                string line = pixels[pixels.Length - 1 - y];
                char c = x < line.Length ? line[x] : '.';
                switch (c)
                {
                    case 'W': return new Color(.84f, .88f, .9f);
                    case 'G': return new Color(.28f, .38f, .46f);
                    case 'D': return new Color(.025f, .06f, .09f);
                    case 'C': return Cyan;
                    case 'c': return new Color(.13f, .47f, .58f);
                    case 'R': return new Color(1, .2f, .24f);
                    case 'O': return Orange;
                    case 'o': return new Color(1, .25f, .1f, .5f);
                    default: return Color.clear;
                }
            }, ppu);
        }

        static SpriteRenderer Draw(string name, Sprite sprite, Vector2 position, Vector2 scale, Color color, int order = -5, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : scenery);
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }
        static SpriteRenderer Rect(string name, float x, float y, float w, float h, Color color, int order = -5, Transform parent = null)
            => Draw(name, white, new Vector2(x, y), new Vector2(w, h), color, order, parent);
        static void Sign(string text, float x, float y, Color color, float size = .13f)
        {
            var go = new GameObject("Señal · " + text);
            go.transform.SetParent(scenery);
            go.transform.position = new Vector3(x, y, 0);
            var label = go.AddComponent<TextMesh>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 60;
            label.characterSize = size * .65f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = color;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = label.font.material;
            renderer.sortingOrder = 2;
        }

        static Color SectorColor(float x) => x < 32 ? Cyan : x < 68 ? Violet : x < 108 ? new Color(.35f, 1, .76f) : x < 146 ? new Color(1, .27f, .26f) : x < 184 ? Orange : new Color(.4f, .65f, 1);
        static void Background()
        {
            for (int x = -4; x < 224; x += 2)
            for (int y = -2; y < 11; y += 2)
                Draw("Panel de pared", panelSprite, new Vector2(x + 1, y + 1), Vector2.one * 2, Color.Lerp(Color.white, SectorColor(x), .12f), -20);
            for (int x = -2; x < 224; x += 6)
            {
                Draw("Conducto vertical", pipeSprite, new Vector2(x, 4), new Vector2(1, 8), Color.white, -15);
                Rect("Viga", x + 1.2f, 4, .16f, 8, new Color(.08f, .13f, .2f), -16);
                Rect("Luz de pared", x + 3, 6.25f, 1.8f, .09f, SectorColor(x), -8);
                Rect("Halo de luz", x + 3, 6.2f, 2.1f, .4f, new Color(.2f, .7f, 1, .035f), -9);
                Rect("Rodapié", x + 3, .3f, 5.9f, .12f, new Color(.1f, .18f, .25f), -12);
            }
            Rect("Tubería horizontal", 110, 6.8f, 228, .17f, new Color(.12f, .2f, .26f), -14);
            Sign("U M B R A L", 7, 4.7f, new Color(.17f, .28f, .37f), .24f);
            Sign("01  /  DESPERTAR", 7, 3.7f, new Color(.23f, .45f, .53f));
            Sign("02  /  CONDUCTOS", 47, 3, new Color(.27f, .22f, .4f), .19f);
            Sign("03  /  CRIOGÉNICO", 87, 3, new Color(.2f, .43f, .37f), .18f);
            Sign("04  /  SEGURIDAD", 124, 3, new Color(.5f, .24f, .26f), .18f);
            Sign("05  /  REACTOR", 170, 4.7f, new Color(.4f, .3f, .23f), .18f);
            Sign("06  /  EVACUACIÓN", 202, 2.9f, new Color(.3f, .43f, .6f), .17f);
            Draw("Cápsula de seguridad", capsuleSprite, new Vector2(.1f, 1.45f), Vector2.one, Color.white, 1);
            for (int i = 0; i < 3; i++)
            {
                float x = 73 + i * 12;
                Rect("Tanque criogénico / carcasa", x, 3.5f, 1.8f, 4.8f, new Color(.14f, .28f, .3f), -10);
                Rect("Tanque criogénico / cristal", x, 3.5f, 1.25f, 4.2f, new Color(.08f, .31f, .32f), -9);
                Rect("Tanque criogénico / fluido", x, 3, .8f, 3, new Color(.16f, .65f, .59f), -8);
                for (int j = 0; j < 3; j++) Draw("Burbujas", ring, new Vector2(x + (j % 2 == 0 ? -.2f : .2f), 2 + j), Vector2.one * .2f, new Color(.5f, 1, .85f), -7);
            }
            for (int x = 111; x < 145; x += 5)
            {
                Rect("Seguridad / línea de alerta", x, 5.6f, 3.8f, .06f, new Color(.65f, .16f, .14f), -7);
                Rect("Seguridad / servidor", x, 2.1f, 1.4f, 3.6f, new Color(.1f, .12f, .17f), -11);
                for (int y = 0; y < 5; y++) Rect("Servidor / estado", x + .3f, .8f + y * .55f, .25f, .08f, y % 2 == 0 ? Orange : Cyan, -10);
            }
            Rect("Reactor / carcasa", 170, 3.6f, 3.5f, 6.6f, new Color(.12f, .18f, .24f), -6);
            Rect("Reactor / núcleo", 170, 3.6f, 1.5f, 6, new Color(.08f, .39f, .5f), -5);
            Rect("Reactor / energía", 170, 3.6f, .5f, 6, Cyan, -4);
            Draw("Reactor / anillo", ring, new Vector2(170, 3.3f), Vector2.one * 3.2f, Cyan, -3);
            for (int i = 0; i < 4; i++) Rect("Reactor / abrazadera", 170, .8f + i * 1.8f, 3.7f, .24f, new Color(.24f, .32f, .39f), -2);
            for (int i = 0; i < 3; i++)
            {
                float x = 189 + i * 11;
                Rect("Hangar / marco de ventanal", x, 4.8f, 8.7f, 4, new Color(.14f, .22f, .32f), -14);
                Rect("Hangar / espacio exterior", x, 4.8f, 8.3f, 3.6f, new Color(.015f, .035f, .085f), -13);
                for (int star = 0; star < 14; star++)
                    Rect("Estrella", x - 3.7f + (star * 1.71f) % 7.5f, 3.25f + (star * .73f) % 3, .025f + (star % 3) * .018f, .045f, new Color(.5f, .72f, .9f), -12);
                Rect("Ventanal / travesaño", x, 4.8f, .1f, 3.6f, new Color(.17f, .25f, .33f), -11);
            }
        }

        static void Floor(int start, int end, int y = -1)
        {
            for (int x = start; x < end; x++) collisionMap.SetTile(new Vector3Int(x, y, 0), floorTile);
        }
        static void Layout()
        {
            Floor(-4, 12); Floor(15, 39); Floor(58, 77); Floor(94, 116); Floor(130, 153); Floor(166, 222);
            Floor(12, 15, 0);
            Floor(-4, 222, 8);
            Rect("Borde inferior del techo", 109, 8.01f, 226, .035f, new Color(.3f, .46f, .57f), 6);
            for (int y = 0; y < 8; y++) { collisionMap.SetTile(new Vector3Int(-4, y, 0), floorTile); collisionMap.SetTile(new Vector3Int(221, y, 0), floorTile); }
            Floor(40, 44, 2); Floor(47, 51, 4); Floor(54, 58, 2);
            Floor(78, 82, 2); Floor(85, 89, 4); Floor(91, 94, 2);
            Floor(118, 122, 2); Floor(125, 128, 4);
            Floor(154, 158, 2); Floor(161, 165, 4);
            Hazards(39, 58); Hazards(77, 94); Hazards(116, 130); Hazards(153, 166);
            Zone(18, 29); Zone(34, 64); Zone(71, 103); Zone(111, 142); Zone(149, 169); Zone(189, 209);
            Module(0, 25, 7, "MARA · El siguiente módulo está en los conductos.");
            Module(1, 49, 7, "NEXO · Activa el laboratorio y elimina el bloqueo de seguridad para llegar al reactor.");
            Module(2, 158, 7, "MARA · Ya tienes los tres. Llévalos a mi consola.");
            Door(31, 0); Door(67, 1); Door(107, 3, "cryo"); Door(145, 4, "security"); Door(177.5f, 2);
            foreach (float x in new[] { 17f, 34, 64, 71, 100, 111, 137, 149, 172, 185, 207 }) Checkpoint(x);
            Hint(3, "NEXO · Ahora puedes moverte más rápido. A/D, SHIFT para correr y ESPACIO para saltar.");
            Hint(6, "NEXO · Mantén J para disparar. Los pulsos destruyen drones y cajas. También puedes disparar en el aire.");
            Hint(18.8f, "NEXO · Q invierte la gravedad en la zona violeta. Recoge el módulo del techo con E.");
            Hint(28, "NEXO · Invierte de nuevo con Q antes de abandonar la zona si quieres bajar.");
            Hint(35, "NEXO · Usa el techo sobre los pinchos. J destruye al Centinela que tienes delante.");
            Hint(60, "NEXO · Activa la palanca con E. La compuerta necesita el segundo módulo.");
            Hint(72, "MARA · Este laboratorio mantiene el reactor congelado. Activa el relé al otro lado del foso.");
            Hint(111, "NEXO · Los Centinelas dorados resisten dos impactos. Destruye el generador rojo para abrir la siguiente puerta.");
            Hint(150, "MARA · El último módulo está sobre el foso del reactor. Instala los tres en mi consola.");
            Hint(186, "MARA · Llegamos al hangar. Despeja la ruta con J; yo estabilizaré el portal.");
            var lever = Prop("Palanca de control", PropKind.Lever, 63, 1, 0);
            Rect("Base de palanca", 63, .45f, .8f, .9f, new Color(.19f, .27f, .34f), 7, lever.transform);
            lever.indicator = Rect("Manija", 63, 1.2f, .15f, .8f, Orange, 8, lever.transform);
            lever.indicator.transform.rotation = Quaternion.Euler(0, 0, -35);
            Sign("E  /  CONTROL", 63, 2.4f, Cyan, .10f);
            var cryo = Prop("Relé criogénico", PropKind.CircuitSwitch, 101, 1, 0);
            cryo.circuit = "cryo";
            Rect("Relé / base", 101, .6f, 1, 1.2f, new Color(.12f, .27f, .29f), 7, cryo.transform);
            cryo.indicator = Rect("Relé / interfaz", 101, 1.3f, 1.2f, .7f, new Color(.3f, .8f, .7f), 8, cryo.transform);
            Sign("E  /  REFRIGERACIÓN", 101, 2.5f, Cyan, .08f);
            Breakable(139.5f, 1, true);
            foreach (float x in new[] { 9f, 98, 188, 197, 208 }) Breakable(x, .65f, false);
            var terminal = Prop("Consola del reactor", PropKind.Reactor, 174, 1.1f, 0);
            Rect("Consola / pedestal", 174, .5f, 1.4f, 1, new Color(.16f, .25f, .32f), 7, terminal.transform);
            Rect("Consola / pantalla", 174, 1.25f, 1.8f, 1, new Color(.05f, .27f, .35f), 7, terminal.transform);
            for (int i = 0; i < 3; i++) Draw("Receptáculo " + (i + 1), module, new Vector2(173.5f + i * .5f, 1.3f), Vector2.one * .5f, Color.white, 8, terminal.transform);
            Sign("E  /  INSTALAR 3 MÓDULOS", 174, 2.5f, Cyan, .09f);
            var portal = Prop("Portal de escape", PropKind.Portal, 214, 1.4f, 0);
            portal.indicator = Draw("Portal / campo", ring, new Vector2(214, 2.4f), new Vector2(2.6f, 4.6f), Cyan, 4, portal.transform);
            Draw("Portal / borde", ring, new Vector2(214, 2.4f), new Vector2(2.9f, 4.9f), new Color(.35f, .46f, .61f), 3, portal.transform);
            Rect("Portal / interior", 214, 2.4f, 1.1f, 3, new Color(.08f, .28f, .38f, .6f), 2, portal.transform);
            Sign("SALIDA", 214, 5.5f, Cyan, .14f);
            GameObject.Find("Señal · SALIDA").transform.SetParent(portal.transform, true);
            var channel = new GameObject("Mara · Punto de canalización");
            channel.transform.SetParent(portal.transform, false); channel.transform.localPosition = new Vector3(-2.7f, -.4f, 0);
            Sign("MARA", 179.5f, 2.3f, Violet, .11f);
            Drone(54.5f, 7.15f, 52, 57, 1.15f);
            Drone(86, 7.15f, 82, 90, 1.3f);
            Drone(122, 7.15f, 119, 126, 1.5f, 2);
            Drone(134, 1, 132, 136, 1.25f, 2);
            Drone(162, 7.15f, 160, 165, 1.4f, 2);
            Drone(191, 1, 190, 194, 1.5f);
            Drone(201, 1, 200, 204, 1.3f, 2);
            Drone(212, 1, 210, 212, 1);
        }
        static void Breakable(float x, float y, bool relay)
        {
            var go = new GameObject(relay ? "Generador de seguridad · 3 impactos" : "Caja destructible");
            go.transform.SetParent(gameplay); go.transform.position = new Vector3(x, y, 0); go.layer = relay ? 7 : 6;
            var collider = go.AddComponent<BoxCollider2D>(); collider.size = new Vector2(1.05f, relay ? 1.8f : 1.3f); collider.isTrigger = relay;
            var health = go.AddComponent<UmbralDamageable>(); health.maxHealth = relay ? 3 : 1; health.countsAsEnemy = false;
            health.circuitOnDestroy = relay ? "security" : ""; health.targetName = relay ? "Generador" : "Caja";
            Rect("Carcasa", x, y, 1.05f, relay ? 1.8f : 1.3f, relay ? new Color(.4f, .12f, .18f) : new Color(.26f, .32f, .4f), 8, go.transform);
            Rect("Marco", x, y, .83f, relay ? 1.5f : 1.05f, new Color(.12f, .18f, .25f), 9, go.transform);
            if (relay)
            {
                Draw("Núcleo de bloqueo", ring, new Vector2(x, y), Vector2.one * .8f, new Color(1, .2f, .18f), 10, go.transform);
                Sign("J  /  DESTRUIR GENERADOR", x, y + 1.6f, Orange, .075f);
            }
            else
            {
                var stripe = Rect("Refuerzo", x, y, .13f, 1.05f, new Color(.61f, .51f, .28f), 10, go.transform);
                stripe.transform.rotation = Quaternion.Euler(0, 0, 35);
            }
            PrefabUtility.SaveAsPrefabAsset(go, Root + (relay ? "/Prefabs/Generador.prefab" : "/Prefabs/Caja.prefab"));
        }

        static UmbralProp Prop(string name, PropKind kind, float x, float y, int index)
        {
            var go = new GameObject(name);
            go.transform.SetParent(gameplay);
            go.transform.position = new Vector3(x, y, 0);
            var prop = go.AddComponent<UmbralProp>();
            prop.kind = kind; prop.index = index;
            return prop;
        }
        static BoxCollider2D Trigger(GameObject go, Vector2 size)
        {
            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true; trigger.size = size;
            return trigger;
        }
        static void Module(int index, float x, float y, string message)
        {
            var prop = Prop("Módulo de energía " + (index + 1), PropKind.Module, x, y, index);
            prop.message = message;
            var visual = new GameObject("Batería"); visual.transform.SetParent(prop.transform, false);
            prop.indicator = visual.AddComponent<SpriteRenderer>(); prop.indicator.sprite = module; prop.indicator.sortingOrder = 10;
            Draw("Halo de módulo", ring, new Vector2(x, y), Vector2.one * 1.2f, new Color(.2f, .8f, 1, .45f), 9, prop.transform);
            Sign("E  /  MÓDULO " + (index + 1), x, y - .8f, Cyan, .08f);
        }
        static void Zone(float left, float right)
        {
            var prop = Prop("Zona de inversión Q", PropKind.GravityZone, (left + right) / 2, 4, 0);
            Trigger(prop.gameObject, new Vector2(right - left, 8));
            Rect("Zona / fondo", (left + right) / 2, 4, right - left, 7.9f, new Color(.23f, .35f, .65f, .055f), -7, prop.transform);
            foreach (var x in new[] { left, right })
            {
                Rect("Límite de gravedad", x, 4, .055f, 7.9f, new Color(.48f, .38f, 1, .6f), 0, prop.transform);
                Sign("Q", x, .65f, Violet, .13f);
                Sign("Q", x, 7.4f, Violet, .13f);
            }
            Sign("INVERSIÓN DE GRAVEDAD", (left + right) / 2, 4.7f, new Color(.33f, .33f, .6f), .085f);
        }
        static void Hazards(int left, int right)
        {
            var prop = Prop("Foso de pinchos", PropKind.Hazard, (left + right) / 2f, -.1f, 0);
            Trigger(prop.gameObject, new Vector2(right - left, .6f));
            for (int x = left; x < right; x++) Draw("Pinchos", spikeSprite, new Vector2(x + .5f, -.1f), Vector2.one, Color.white, 6, prop.transform);
            Floor(left, right, -2);
        }
        static void Door(float x, int index, string circuit = "")
        {
            var prop = Prop(index == 2 ? "Barrera de Mara" : "Compuerta " + (index + 1), PropKind.Door, x, 4, index);
            prop.circuit = circuit;
            prop.gameObject.layer = 6;
            var collider = prop.gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(.5f, 8);
            prop.barrier = collider;
            var color = index == 2 ? Violet : Cyan;
            Rect("Campo de compuerta", x, 4, .3f, 8, new Color(color.r, color.g, color.b, .43f), 6, prop.transform);
            Rect("Marco izquierdo", x - .4f, 4, .18f, 8, new Color(.25f, .36f, .44f), 6, prop.transform);
            Rect("Marco derecho", x + .4f, 4, .18f, 8, new Color(.25f, .36f, .44f), 6, prop.transform);
            for (int y = 0; y < 8; y++) Rect("Bandas de energía", x, y + .4f, .48f, .035f, color, 7, prop.transform);
            if (index < 2) Sign(index == 0 ? "REQUIERE MÓDULO 1" : "MÓDULO 2 + PALANCA", x - 1.8f, 2.8f, color, .065f);
        }
        static void Checkpoint(float x)
        {
            var prop = Prop("Baliza de control", PropKind.Checkpoint, x, 1, 0);
            Trigger(prop.gameObject, new Vector2(1, 2));
            Rect("Baliza / base", x, .18f, .7f, .36f, new Color(.21f, .29f, .37f), 7, prop.transform);
            prop.indicator = Rect("Baliza / luz", x, .48f, .18f, .5f, new Color(.19f, .4f, .5f), 8, prop.transform);
        }
        static void Hint(float x, string message)
        {
            var prop = Prop("Mensaje de radio", PropKind.Hint, x, 4, 0);
            prop.message = message;
            Trigger(prop.gameObject, new Vector2(1, 8));
        }
        static UmbralPlayer CreatePlayer(PhysicsMaterial2D material)
        {
            var go = new GameObject("Leo · Protagonista");
            go.transform.position = new Vector3(2, 1, 0);
            go.tag = "Player";
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 2;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = go.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(.63f, 1.48f);
            collider.sharedMaterial = material;
            var visualObject = new GameObject("Leo · Animación");
            visualObject.transform.SetParent(go.transform, false);
            var renderer = visualObject.AddComponent<SpriteRenderer>();
            renderer.sprite = leo[0]; renderer.sortingOrder = 15;
            var player = go.AddComponent<UmbralPlayer>();
            player.visual = renderer; player.frames = leo; player.character = leoDefinition; player.settings = balance; player.ApplyCharacter();
            PrefabUtility.SaveAsPrefabAsset(go, Root + "/Prefabs/Leo.prefab");
            return player;
        }
        static void Companion(bool isMara, Vector2 position, Sprite sprite)
        {
            var visual = Draw(isMara ? "Mara · Aliada" : "Nexo · Robot guía", sprite, position, Vector2.one, Color.white, 12, gameplay);
            var companion = visual.gameObject.AddComponent<UmbralCompanion>();
            companion.isMara = isMara; companion.visual = visual; companion.frames = isMara ? mara : null; companion.character = isMara ? maraDefinition : null;
            if (isMara) PrefabUtility.SaveAsPrefabAsset(visual.gameObject, Root + "/Prefabs/Mara.prefab");
        }
        static void Drone(float x, float y, float left, float right, float speed, int health = 1)
        {
            var visual = Draw("Centinela · Patrulla", droneSprite, new Vector2(x, y), Vector2.one, Color.white, 13, gameplay);
            visual.gameObject.layer = 7;
            var damageable = visual.gameObject.AddComponent<UmbralDamageable>(); damageable.maxHealth = health;
            damageable.targetName = health > 1 ? "Centinela reforzado" : "Centinela";
            if (health > 1) visual.color = new Color(1, .78f, .4f);
            Trigger(visual.gameObject, new Vector2(.95f, .65f));
            var drone = visual.gameObject.AddComponent<UmbralDrone>();
            drone.left = new Vector2(left, y); drone.right = new Vector2(right, y); drone.eye = visual; drone.speed = speed;
            PrefabUtility.SaveAsPrefabAsset(visual.gameObject, Root + (health > 1 ? "/Prefabs/CentinelaReforzado.prefab" : "/Prefabs/Centinela.prefab"));
        }

        [MenuItem("Umbral/Comprobar escena")]
        public static void Validate()
        {
            var props = UnityEngine.Object.FindObjectsByType<UmbralProp>(FindObjectsSortMode.None);
            if (props.Count(p => p.kind == PropKind.Module) != 3) throw new Exception("Deben existir tres módulos.");
            if (props.Count(p => p.kind == PropKind.GravityZone) < 6) throw new Exception("Deben existir al menos seis zonas de gravedad.");
            if (UnityEngine.Object.FindFirstObjectByType<UmbralPlayer>() == null) throw new Exception("Falta Leo.");
            if (UnityEngine.Object.FindFirstObjectByType<UmbralGame>() == null) throw new Exception("Falta el control de misión.");
            var map = UnityEngine.Object.FindFirstObjectByType<Tilemap>();
            if (!map.HasTile(new Vector3Int(158, 8, 0))) throw new Exception("La ruta al módulo 3 necesita techo sólido.");
            if (props.Single(p => p.kind == PropKind.Module && p.index == 2).transform.position.x >= 177.5f) throw new Exception("El tercer módulo debe preceder a la barrera de Mara.");
            Debug.Log("UMBRAL_SCENE_VALIDATED_OK");
        }

        [MenuItem("Umbral/Compilar Windows")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory("Builds/Windows");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/Windows/Proyecto Umbral.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
            });
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("La compilación Windows no terminó correctamente: " + result.summary.result);
            Debug.Log("UMBRAL_WINDOWS_BUILD_OK");
        }
    }
}

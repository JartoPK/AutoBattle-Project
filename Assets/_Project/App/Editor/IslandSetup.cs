using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AutoBattle.App.Editor
{
    public static class IslandSetup
    {
        private const string TilesDir = "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Sliced Tiles";
        private const string FoamTexPath = "Assets/Tiny Swords/Terrain/Tileset/Water Foam.png";
        private const string WaterTilePath = "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Tile animated.asset";
        private const string WaterBgTilePath = "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Water Background color.asset";
        private const string ShadowTilePath = "Assets/Tiny Swords/Terrain/Tileset/Tilemap Settings/Shadow.asset";
        private const string Res = "Assets/_Project/Resources";
        private const string DecoTilesDir = "Assets/_Project/Resources/DecoTiles";
        private const string ScenesDir = "Assets/_Project/Scenes";
        private const string PaletteDir = "Assets/_Project/TilePalettes";

        private const string DecorationsDir = "Assets/Tiny Swords/Terrain/Decorations";

        private static readonly (string path, string label)[] DecoGroups_Animated = new[]
        {
            ("Assets/Tiny Swords/Terrain/Decorations/Bushes/Bush 1.png", "Bush_1"),
            ("Assets/Tiny Swords/Terrain/Decorations/Bushes/Bush 2.png", "Bush_2"),
            ("Assets/Tiny Swords/Terrain/Decorations/Bushes/Bush 3.png", "Bush_3"),
            ("Assets/Tiny Swords/Terrain/Decorations/Bushes/Bush 4.png", "Bush_4"),

            ("Assets/Tiny Swords/Terrain/Decorations/Rocks in the Water/Water Rocks_01.png", "WaterRock_1"),
            ("Assets/Tiny Swords/Terrain/Decorations/Rocks in the Water/Water Rocks_02.png", "WaterRock_2"),
            ("Assets/Tiny Swords/Terrain/Decorations/Rocks in the Water/Water Rocks_03.png", "WaterRock_3"),
            ("Assets/Tiny Swords/Terrain/Decorations/Rocks in the Water/Water Rocks_04.png", "WaterRock_4"),

            ("Assets/Tiny Swords/Pawn and Resources/Wood/Trees/Tree1.png", "Tree_1"),
            ("Assets/Tiny Swords/Pawn and Resources/Wood/Trees/Tree2.png", "Tree_2"),
            ("Assets/Tiny Swords/Pawn and Resources/Wood/Trees/Tree3.png", "Tree_3"),
            ("Assets/Tiny Swords/Pawn and Resources/Wood/Trees/Tree4.png", "Tree_4"),
        };

        private static readonly (string path, string label)[] DecoGroups_Static = new[]
        {
            ("Assets/Tiny Swords/Terrain/Decorations/Rocks/Rock1.png", "Rock_1"),
            ("Assets/Tiny Swords/Terrain/Decorations/Rocks/Rock2.png", "Rock_2"),
            ("Assets/Tiny Swords/Terrain/Decorations/Rocks/Rock3.png", "Rock_3"),
            ("Assets/Tiny Swords/Terrain/Decorations/Rocks/Rock4.png", "Rock_4"),
        };

        private const int LayerCount = 6;
        private const int BaseW = 18;
        private const int BaseH = 11;
        private const int MapW = 30;
        private const int MapH = 20;

        [MenuItem("AutoBattle/Setup/Añadir nodo de nivel al mapa")]
        public static void AddMapNode()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "Map")
            {
                Debug.LogError("[AutoBattle] Abre Map.unity primero.");
                return;
            }

            var go = new GameObject("Nivel_Nuevo");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateNodeSprite();
            sr.color = new Color(0.85f, 0.80f, 0.5f);
            sr.sortingOrder = 2000;
            go.transform.localScale = Vector3.one * 1.5f;
            go.AddComponent<MapNode>();

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                var camPos = sceneView.camera.transform.position;
                go.transform.position = new Vector3(camPos.x, camPos.y, 0f);
            }

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[AutoBattle] Nodo creado. Asigna un CampaignNodeData en el Inspector (componente MapNode) y muévelo donde quieras.");
        }

        [MenuItem("AutoBattle/Setup/Añadir capa Decoration a escena actual")]
        public static void AddDecorationLayer()
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<Grid>() == null) continue;

                var existing = root.transform.Find("Decoration");
                if (existing != null)
                {
                    Debug.Log("[AutoBattle] La capa Decoration ya existe en esta escena.");
                    return;
                }

                CreateTilemapLayer(root.transform, "Decoration", 600);
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log("[AutoBattle] Capa Decoration añadida. Usa Tile Palette > Active Tilemap > Decoration para pintar decoraciones.");
                return;
            }

            Debug.LogError("[AutoBattle] No se encontró un Grid en la escena. Abre Base.unity o Map.unity primero.");
        }

        [MenuItem("AutoBattle/Setup/Recrear escena Map (BORRA la actual)")]
        public static void RecreateMapScene()
        {
            if (!EditorUtility.DisplayDialog("Recrear Map",
                "Esto BORRA Map.unity y la crea de nuevo con tilemaps.\n¿Continuar?", "Sí", "Cancelar"))
                return;

            var mapPath = $"{ScenesDir}/Map.unity";
            if (System.IO.File.Exists(mapPath))
                AssetDatabase.DeleteAsset(mapPath);

            var allTiles = new Dictionary<string, Dictionary<(int, int), TileBase>>();
            for (int c = 1; c <= 5; c++)
            {
                var grid = LoadTileGrid($"Tilemap_color{c}");
                if (grid.Count > 0) allTiles[$"color{c}"] = grid;
            }
            var color1 = allTiles.Count > 0 ? allTiles.Values.First() : new Dictionary<(int, int), TileBase>();

            var foamTile = CreateFoamAnimatedTile();
            var waterTileAnimated = AssetDatabase.LoadAssetAtPath<TileBase>(WaterTilePath);
            var waterTileBg = AssetDatabase.LoadAssetAtPath<TileBase>(WaterBgTilePath);

            CreateMapScene(color1, foamTile, waterTileAnimated, waterTileBg);
            AddScenesToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(mapPath);
            Debug.Log("[AutoBattle] Map.unity recreada con tilemaps. Usa 'Añadir nodo de nivel al mapa' para colocar niveles.");
        }

        [MenuItem("AutoBattle/Setup/Añadir capa NoWalk a escena actual")]
        public static void AddNoWalkLayer()
        {
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<Grid>() == null) continue;

                var existing = root.transform.Find("NoWalk");
                if (existing != null)
                {
                    Debug.Log("[AutoBattle] La capa NoWalk ya existe en esta escena.");
                    return;
                }

                var go = CreateTilemapLayer(root.transform, "NoWalk", 999);
                go.GetComponent<TilemapRenderer>().sortingOrder = 999;
                go.GetComponent<Tilemap>().color = new Color(1f, 0f, 0f, 0.35f);
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log("[AutoBattle] Capa NoWalk añadida. Usa Tile Palette > Active Tilemap > NoWalk para pintar zonas bloqueadas.");
                return;
            }

            Debug.LogError("[AutoBattle] No se encontró un Grid en la escena. Abre Base.unity o Map.unity primero.");
        }

        [MenuItem("AutoBattle/Setup/Crear escenas (Map + Base)")]
        public static void Setup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolder(Res);
            EnsureFolder(ScenesDir);
            EnsureFolder(PaletteDir);

            var allTiles = new Dictionary<string, Dictionary<(int, int), TileBase>>();
            for (int c = 1; c <= 5; c++)
            {
                var grid = LoadTileGrid($"Tilemap_color{c}");
                if (grid.Count > 0) allTiles[$"color{c}"] = grid;
            }

            if (allTiles.Count == 0)
            {
                Debug.LogError("[AutoBattle] No se encontraron tiles de terreno.");
                return;
            }

            var foamTile = CreateFoamAnimatedTile();
            var waterTileAnimated = AssetDatabase.LoadAssetAtPath<TileBase>(WaterTilePath);
            var waterTileBg = AssetDatabase.LoadAssetAtPath<TileBase>(WaterBgTilePath);
            var shadowTile = AssetDatabase.LoadAssetAtPath<TileBase>(ShadowTilePath);
            var color1 = allTiles.Values.First();

            var mapPath = $"{ScenesDir}/Map.unity";
            var basePath = $"{ScenesDir}/Base.unity";

            if (!System.IO.File.Exists(mapPath))
            {
                CreateMapScene(color1, foamTile, waterTileAnimated, waterTileBg);
                Debug.Log("[AutoBattle] Map.unity creada.");
            }
            else
            {
                Debug.Log("[AutoBattle] Map.unity ya existe, no se sobreescribe.");
            }

            if (!System.IO.File.Exists(basePath))
            {
                CreateBaseScene(color1, foamTile, waterTileAnimated, waterTileBg);
                Debug.Log("[AutoBattle] Base.unity creada.");
            }
            else
            {
                Debug.Log("[AutoBattle] Base.unity ya existe, no se sobreescribe.");
            }

            CreatePalette(allTiles, foamTile, waterTileAnimated, waterTileBg, shadowTile);
            AddScenesToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[AutoBattle] Setup completo.\n" +
                      $"  Palette: {PaletteDir}/TerrainPalette.prefab\n" +
                      "  Las escenas existentes NO se han tocado.\n" +
                      "  Para RECREAR una escena, borra el .unity y ejecuta de nuevo.");
        }

        // ── Map Scene (2D con tilemaps) ────────────────────────────────

        private static void CreateMapScene(
            Dictionary<(int, int), TileBase> terrainTiles,
            TileBase foamTile,
            TileBase waterTileAnimated,
            TileBase waterTileBg)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGO = new GameObject("PreviewCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 12f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.25f, 0.40f);
            camGO.transform.position = new Vector3(0, 0, -10);

            var gridGO = new GameObject("MapIsland");
            var grid = gridGO.AddComponent<Grid>();
            grid.cellSize = new Vector3(1, 1, 0);

            for (int i = 0; i < LayerCount; i++)
                CreateTilemapLayer(gridGO.transform, $"Layer {i}", i * 100);
            CreateTilemapLayer(gridGO.transform, "Decoration", 600);

            var layer0 = gridGO.transform.Find("Layer 0").GetComponent<Tilemap>();
            var layer1 = gridGO.transform.Find("Layer 1").GetComponent<Tilemap>();
            var layer2 = gridGO.transform.Find("Layer 2").GetComponent<Tilemap>();

            var waterTile = waterTileAnimated != null ? waterTileAnimated : waterTileBg;
            PaintRect(layer0, waterTile, MapW, MapH, 4);
            PaintBorder(layer1, foamTile, MapW, MapH);
            PaintGround(layer2, terrainTiles, MapW, MapH);

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/Map.unity");
        }

        // ── Base Scene ─────────────────────────────────────────────────

        private static void CreateBaseScene(
            Dictionary<(int, int), TileBase> terrainTiles,
            TileBase foamTile,
            TileBase waterTileAnimated,
            TileBase waterTileBg)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGO = new GameObject("PreviewCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.40f, 0.55f);
            camGO.transform.position = new Vector3(0, 0, -10);

            var gridGO = new GameObject("BaseIsland");
            var grid = gridGO.AddComponent<Grid>();
            grid.cellSize = new Vector3(1, 1, 0);

            for (int i = 0; i < LayerCount; i++)
                CreateTilemapLayer(gridGO.transform, $"Layer {i}", i * 100);
            CreateTilemapLayer(gridGO.transform, "Decoration", 600);

            var layer0 = gridGO.transform.Find("Layer 0").GetComponent<Tilemap>();
            var layer1 = gridGO.transform.Find("Layer 1").GetComponent<Tilemap>();
            var layer2 = gridGO.transform.Find("Layer 2").GetComponent<Tilemap>();

            var waterTile = waterTileAnimated != null ? waterTileAnimated : waterTileBg;
            PaintRect(layer0, waterTile, BaseW, BaseH, 3);
            PaintBorder(layer1, foamTile, BaseW, BaseH);
            PaintGround(layer2, terrainTiles, BaseW, BaseH);

            var noWalkGO = CreateTilemapLayer(gridGO.transform, "NoWalk", 999);
            noWalkGO.GetComponent<TilemapRenderer>().enabled = false;

            var art = AssetDatabase.LoadAssetAtPath<ArtConfig>($"{Res}/ArtConfig.asset");
            if (art != null)
            {
                PlaceBuilding(gridGO.transform, "Cuartel", art.house, new Vector3(-6f, 2.2f, 0), 0.7f);
                PlaceBuilding(gridGO.transform, "Herencia", art.monastery, new Vector3(6f, 2.2f, 0), 0.7f);
                PlaceBuilding(gridGO.transform, "Reclutamiento", art.barracks, new Vector3(-3.5f, -2.2f, 0), 0.8f);
                PlaceBuilding(gridGO.transform, "Mejoras", art.castle, new Vector3(4f, -2.2f, 0), 0.8f);
            }

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/Base.unity");
        }

        // ── Build Settings ─────────────────────────────────────────────

        private static void AddScenesToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var mapPath = $"{ScenesDir}/Map.unity";
            var basePath = $"{ScenesDir}/Base.unity";

            if (!scenes.Any(s => s.path == mapPath))
                scenes.Add(new EditorBuildSettingsScene(mapPath, true));
            if (!scenes.Any(s => s.path == basePath))
                scenes.Add(new EditorBuildSettingsScene(basePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ── Tilemap paint helpers ──────────────────────────────────────

        private static GameObject CreateTilemapLayer(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            return go;
        }

        private static void PaintRect(Tilemap tm, TileBase tile, int w, int h, int margin)
        {
            if (tile == null) return;
            int ox = w / 2, oy = h / 2;
            for (int x = -margin; x < w + margin; x++)
            for (int y = -margin; y < h + margin; y++)
                tm.SetTile(new Vector3Int(x - ox, y - oy, 0), tile);
        }

        private static void PaintBorder(Tilemap tm, TileBase foamTile, int w, int h)
        {
            if (foamTile == null) return;
            int ox = w / 2, oy = h / 2;
            for (int x = -1; x <= w; x++)
            {
                tm.SetTile(new Vector3Int(x - ox, -1 - oy, 0), foamTile);
                tm.SetTile(new Vector3Int(x - ox, h - oy, 0), foamTile);
            }
            for (int y = 0; y < h; y++)
            {
                tm.SetTile(new Vector3Int(-1 - ox, y - oy, 0), foamTile);
                tm.SetTile(new Vector3Int(w - ox, y - oy, 0), foamTile);
            }
        }

        private static void PaintGround(Tilemap tm, Dictionary<(int, int), TileBase> tiles, int w, int h)
        {
            TileBase T(int c, int r) => tiles.TryGetValue((c, r), out var t) ? t : null;

            var center = T(1, 4); var top = T(1, 5); var bottom = T(1, 3);
            var left = T(0, 4); var right = T(2, 4);
            var tl = T(0, 5); var tr = T(2, 5); var bl = T(0, 3); var br = T(2, 3);

            int ox = w / 2, oy = h / 2;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                bool isT = y == h - 1, isB = y == 0, isL = x == 0, isR = x == w - 1;
                TileBase tile;
                if (isT && isL) tile = tl; else if (isT && isR) tile = tr;
                else if (isB && isL) tile = bl; else if (isB && isR) tile = br;
                else if (isT) tile = top; else if (isB) tile = bottom;
                else if (isL) tile = left; else if (isR) tile = right;
                else tile = center;
                if (tile != null) tm.SetTile(new Vector3Int(x - ox, y - oy, 0), tile);
            }
        }

        private static void PlaceBuilding(Transform parent, string name, Sprite sprite, Vector3 pos, float scale)
        {
            if (sprite == null) return;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1000 + Mathf.RoundToInt(-pos.y * 100f);
        }

        // ── Animated Foam Tile ─────────────────────────────────────────

        private static TileBase CreateFoamAnimatedTile()
        {
            var sprites = LoadFoamSprites();
            if (sprites == null || sprites.Length == 0) return null;

            var tilePath = $"{Res}/FoamAnimatedTile.asset";
            var waterRef = AssetDatabase.LoadAssetAtPath<Object>(WaterTilePath);
            if (waterRef == null)
            {
                var fallback = ScriptableObject.CreateInstance<Tile>();
                fallback.sprite = sprites[0];
                AssetDatabase.CreateAsset(fallback, tilePath);
                return fallback;
            }

            var existing = AssetDatabase.LoadAssetAtPath<Object>(tilePath);
            if (existing != null) AssetDatabase.DeleteAsset(tilePath);

            var copy = Object.Instantiate(waterRef);
            copy.name = "FoamAnimatedTile";
            AssetDatabase.CreateAsset(copy, tilePath);

            var so = new SerializedObject(copy);
            var arr = so.FindProperty("m_AnimatedSprites");
            arr.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            so.FindProperty("m_MinSpeed").floatValue = 8f;
            so.FindProperty("m_MaxSpeed").floatValue = 10f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(copy);

            return copy as TileBase;
        }

        // ── Tile Palette ───────────────────────────────────────────────

        private static void CreatePalette(
            Dictionary<string, Dictionary<(int, int), TileBase>> allTiles,
            TileBase foamTile,
            TileBase waterTileAnimated,
            TileBase waterTileBg,
            TileBase shadowTile)
        {
            // ── TerrainPalette (solo terreno) ──
            var palettePath = $"{PaletteDir}/TerrainPalette.prefab";

            if (AssetDatabase.LoadAssetAtPath<Object>(palettePath) != null)
                AssetDatabase.DeleteAsset(palettePath);

            var paletteGO = new GameObject("TerrainPalette");
            var grid = paletteGO.AddComponent<Grid>();
            grid.cellSize = new Vector3(1, 1, 0);

            var layerGO = new GameObject("Layer1");
            layerGO.transform.SetParent(paletteGO.transform, false);
            var tm = layerGO.AddComponent<Tilemap>();
            layerGO.AddComponent<TilemapRenderer>();

            int rowOffset = 0;
            foreach (var kv in allTiles)
            {
                foreach (var tileKv in kv.Value)
                    tm.SetTile(new Vector3Int(tileKv.Key.Item1, tileKv.Key.Item2 + rowOffset, 0), tileKv.Value);

                int maxRow = 0;
                foreach (var tileKv in kv.Value)
                    if (tileKv.Key.Item2 > maxRow) maxRow = tileKv.Key.Item2;
                rowOffset += maxRow + 2;
            }

            if (foamTile != null)
            {
                tm.SetTile(new Vector3Int(0, rowOffset, 0), foamTile);
                tm.SetTile(new Vector3Int(1, rowOffset, 0), foamTile);
                tm.SetTile(new Vector3Int(2, rowOffset, 0), foamTile);
                rowOffset += 2;
            }

            if (waterTileAnimated != null)
                tm.SetTile(new Vector3Int(0, rowOffset, 0), waterTileAnimated);
            if (waterTileBg != null)
                tm.SetTile(new Vector3Int(1, rowOffset, 0), waterTileBg);
            if (shadowTile != null)
                tm.SetTile(new Vector3Int(2, rowOffset, 0), shadowTile);

            SavePalettePrefab(paletteGO, palettePath);

            // ── DecorationPalette (solo decoraciones) ──
            EnsureFolder(DecoTilesDir);
            var decoTileMap = CreateDecorationTiles();

            var decoPalettePath = $"{PaletteDir}/DecorationPalette.prefab";

            if (AssetDatabase.LoadAssetAtPath<Object>(decoPalettePath) != null)
                AssetDatabase.DeleteAsset(decoPalettePath);

            var decoPaletteGO = new GameObject("DecorationPalette");
            var decoGrid = decoPaletteGO.AddComponent<Grid>();
            decoGrid.cellSize = new Vector3(1, 1, 0);

            var decoLayerGO = new GameObject("Layer1");
            decoLayerGO.transform.SetParent(decoPaletteGO.transform, false);
            var decoTm = decoLayerGO.AddComponent<Tilemap>();
            decoLayerGO.AddComponent<TilemapRenderer>();

            int row = 0;
            PlaceDecoRow(decoTm, decoTileMap, "Bush", ref row);
            row += 3;
            PlaceDecoRow(decoTm, decoTileMap, "Rock_", ref row);
            row += 3;
            PlaceDecoRow(decoTm, decoTileMap, "WaterRock", ref row);
            row += 3;
            PlaceDecoRow(decoTm, decoTileMap, "Tree", ref row);

            SavePalettePrefab(decoPaletteGO, decoPalettePath);
        }

        private static void SavePalettePrefab(GameObject paletteGO, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(paletteGO, path);
            Object.DestroyImmediate(paletteGO);

            var gp = ScriptableObject.CreateInstance<GridPalette>();
            gp.name = "GridPalette";
            gp.cellSizing = GridPalette.CellSizing.Automatic;
            AssetDatabase.AddObjectToAsset(gp, path);
            AssetDatabase.SaveAssets();
        }

        // ── Decoration Tiles ──────────────────────────────────────────

        private static void PlaceDecoRow(Tilemap tm, Dictionary<string, TileBase> tiles, string prefix, ref int row)
        {
            int col = 0;
            foreach (var kv in tiles)
            {
                if (!kv.Key.StartsWith(prefix)) continue;
                tm.SetTile(new Vector3Int(col * 3, row, 0), kv.Value);
                col++;
            }
        }

        private static Dictionary<string, TileBase> CreateDecorationTiles()
        {
            var waterRef = AssetDatabase.LoadAssetAtPath<Object>(WaterTilePath);
            var result = new Dictionary<string, TileBase>();

            foreach (var (path, label) in DecoGroups_Animated)
            {
                var sprites = LoadSortedSubSprites(path);
                if (sprites == null || sprites.Length == 0) continue;

                var tilePath = $"{DecoTilesDir}/{label}_anim.asset";

                var existing = AssetDatabase.LoadAssetAtPath<Object>(tilePath);
                if (existing != null) AssetDatabase.DeleteAsset(tilePath);

                if (waterRef != null)
                {
                    var copy = Object.Instantiate(waterRef);
                    copy.name = label;
                    AssetDatabase.CreateAsset(copy, tilePath);

                    var so = new SerializedObject(copy);
                    var arr = so.FindProperty("m_AnimatedSprites");
                    arr.arraySize = sprites.Length;
                    for (int i = 0; i < sprites.Length; i++)
                        arr.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
                    so.FindProperty("m_MinSpeed").floatValue = 6f;
                    so.FindProperty("m_MaxSpeed").floatValue = 8f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(copy);

                    var tile = copy as TileBase;
                    if (tile != null) result[label] = tile;
                }
                else
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprites[0];
                    tile.name = label;
                    AssetDatabase.CreateAsset(tile, tilePath);
                    result[label] = tile;
                }
            }

            foreach (var (path, label) in DecoGroups_Static)
            {
                var sprites = LoadSortedSubSprites(path);
                if (sprites == null || sprites.Length == 0) continue;

                var tilePath = $"{DecoTilesDir}/{label}.asset";

                var existing = AssetDatabase.LoadAssetAtPath<Object>(tilePath);
                if (existing != null) AssetDatabase.DeleteAsset(tilePath);

                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprites[0];
                tile.name = label;
                AssetDatabase.CreateAsset(tile, tilePath);
                result[label] = tile;
            }

            return result;
        }

        private static Sprite[] LoadSortedSubSprites(string path)
        {
            var list = new List<Sprite>();
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                if (o is Sprite s) list.Add(s);
            list.Sort((a, b) => FrameIndex(a.name).CompareTo(FrameIndex(b.name)));
            return list.ToArray();
        }

        // ── Utility ────────────────────────────────────────────────────

        private static Dictionary<(int, int), TileBase> LoadTileGrid(string prefix)
        {
            var result = new Dictionary<(int, int), TileBase>();
            for (int i = 0; i <= 43; i++)
            {
                var path = $"{TilesDir}/{prefix}_{i}.asset";
                var tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                if (tile == null) continue;
                if (tile is Tile t && t.sprite != null)
                {
                    var r = t.sprite.rect;
                    int col = Mathf.RoundToInt(r.x / 64f);
                    int row = Mathf.RoundToInt(r.y / 64f);
                    result[(col, row)] = tile;
                }
            }
            return result;
        }

        private static Sprite[] LoadFoamSprites()
        {
            var list = new List<Sprite>();
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(FoamTexPath))
                if (o is Sprite s) list.Add(s);
            list.Sort((a, b) => FrameIndex(a.name).CompareTo(FrameIndex(b.name)));
            return list.ToArray();
        }

        private static int FrameIndex(string name)
        {
            int i = name.LastIndexOf('_');
            return i >= 0 && int.TryParse(name.Substring(i + 1), out var n) ? n : 0;
        }

        private static Sprite GetOrCreateNodeSprite()
        {
            var spritePath = $"{Res}/NodeMarker.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (existing != null) return existing;

            var tex = new Texture2D(4, 4);
            for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                tex.SetPixel(x, y, Color.white);
            tex.Apply();
            System.IO.File.WriteAllBytes(spritePath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(spritePath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 4;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}

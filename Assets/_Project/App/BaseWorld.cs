using System;
using System.Collections.Generic;
using AutoBattle.Core.Units;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace AutoBattle.App
{
    public class BaseWorld
    {
        public readonly GameObject Root;

        private const float InteriorX = 6.5f;
        private const float InteriorY = 3.0f;

        private static readonly Vector3 CamPos = new(0f, 0f, -10f);

        private readonly ArtConfig _art;
        private readonly Action _onRecruitClicked;
        private readonly Action _onUpgradesClicked;
        private readonly Transform _troopsRoot;
        private readonly Dictionary<string, GameObject> _troops = new();
        private readonly Dictionary<string, bool> _troopHasClass = new();
        private Wanderer2D.Obstacle[] _obstaclesArr = Array.Empty<Wanderer2D.Obstacle>();
        private Tilemap _noWalkTilemap;

        public BaseWorld(Transform parent, ArtConfig art, Action onRecruitClicked, Action onUpgradesClicked)
        {
            _art = art;
            _onRecruitClicked = onRecruitClicked;
            _onUpgradesClicked = onUpgradesClicked;

            Root = new GameObject("BaseWorld2D");
            Root.transform.SetParent(parent, false);
            Root.SetActive(false);

            _troopsRoot = new GameObject("Troops").transform;
            _troopsRoot.SetParent(Root.transform, false);
        }

        public void OnSceneLoaded()
        {
            var scene = SceneManager.GetSceneByName("Base");
            if (!scene.isLoaded) return;

            var obstacles = new List<Wanderer2D.Obstacle>();
            _noWalkTilemap = null;

            foreach (var root in scene.GetRootGameObjects())
            {
                WireBuilding(root.transform, "Reclutamiento", _onRecruitClicked, obstacles);
                WireBuilding(root.transform, "Mejoras", _onUpgradesClicked, obstacles);
                AddObstacle(root.transform, "Cuartel", obstacles);
                AddObstacle(root.transform, "Herencia", obstacles);

                var cam = root.GetComponent<Camera>();
                if (cam != null) cam.enabled = false;

                var nw = FindChild(root.transform, "NoWalk");
                if (nw != null)
                {
                    _noWalkTilemap = nw.GetComponent<Tilemap>();
                    var renderer = nw.GetComponent<TilemapRenderer>();
                    if (renderer != null) renderer.enabled = false;
                }
            }

            _obstaclesArr = obstacles.ToArray();
        }

        public void Show(Camera cam)
        {
            Root.SetActive(true);
            cam.orthographic = true;
            cam.orthographicSize = 6.5f;
            cam.transform.SetPositionAndRotation(CamPos, Quaternion.identity);
        }

        public void Hide()
        {
            Root.SetActive(false);
            ClearTroops();
        }

        public void RefreshTroops(Roster roster)
        {
            var present = new HashSet<string>();
            foreach (var u in roster.units) present.Add(u.id);

            var remove = new List<string>();
            foreach (var kv in _troops)
            {
                bool gone = !present.Contains(kv.Key);
                bool changed = false;
                if (!gone)
                {
                    var unit = roster.Get(kv.Key);
                    changed = unit != null && _troopHasClass.TryGetValue(kv.Key, out var had) && had != unit.hasClass;
                }
                if (gone || changed) { UnityEngine.Object.Destroy(kv.Value); remove.Add(kv.Key); }
            }
            foreach (var id in remove) { _troops.Remove(id); _troopHasClass.Remove(id); }

            foreach (var u in roster.units)
            {
                if (_troops.ContainsKey(u.id)) continue;
                _troops[u.id] = CreateTroop(u);
                _troopHasClass[u.id] = u.hasClass;
            }
        }

        private void ClearTroops()
        {
            foreach (var kv in _troops) UnityEngine.Object.Destroy(kv.Value);
            _troops.Clear();
            _troopHasClass.Clear();
        }

        private GameObject CreateTroop(UnitInstance u)
        {
            var anim = u.hasClass && _art != null ? _art.AnimFor(u.classId) : (_art != null ? _art.pawn : null);

            var pos = SpawnPos();
            var go = new GameObject($"Troop_{u.displayName}");
            go.transform.SetParent(_troopsRoot, false);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = Vector3.one * 0.5f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1500;
            var sa = go.AddComponent<SpriteAnimator>();
            if (anim != null) { sa.idle = anim.idle; sa.run = anim.run; }

            var w = go.AddComponent<Wanderer2D>();
            w.obstacles = _obstaclesArr;
            w.noWalkTilemap = _noWalkTilemap;
            w.areaX = InteriorX;
            w.areaY = InteriorY;
            return go;
        }

        private static void WireBuilding(Transform sceneRoot, string name, Action onClick, List<Wanderer2D.Obstacle> obstacles)
        {
            var t = FindChild(sceneRoot, name);
            if (t == null) return;

            var pos = t.position;
            obstacles.Add(new Wanderer2D.Obstacle { center = new Vector2(pos.x, pos.y), radius = 1.5f });

            if (t.GetComponent<BoxCollider2D>() == null)
            {
                var col = t.gameObject.AddComponent<BoxCollider2D>();
                var sr = t.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null) col.size = sr.sprite.bounds.size;
            }

            if (t.GetComponent<ClickableBuilding>() == null)
                t.gameObject.AddComponent<ClickableBuilding>().OnClicked = onClick;
        }

        private static void AddObstacle(Transform sceneRoot, string name, List<Wanderer2D.Obstacle> obstacles)
        {
            var t = FindChild(sceneRoot, name);
            if (t == null) return;
            var pos = t.position;
            obstacles.Add(new Wanderer2D.Obstacle { center = new Vector2(pos.x, pos.y), radius = 1.5f });
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private Vector3 SpawnPos()
        {
            for (int i = 0; i < 18; i++)
            {
                var p = new Vector3(UnityEngine.Random.Range(-InteriorX, InteriorX), UnityEngine.Random.Range(-InteriorY, InteriorY), 0f);
                if (!InObstacle(p)) return p;
            }
            return new Vector3(0f, InteriorY, 0f);
        }

        private bool InObstacle(Vector3 p)
        {
            var xy = new Vector2(p.x, p.y);
            foreach (var o in _obstaclesArr)
                if (Vector2.Distance(xy, o.center) < o.radius) return true;

            if (_noWalkTilemap != null)
            {
                var cell = _noWalkTilemap.WorldToCell(p);
                if (_noWalkTilemap.HasTile(cell)) return true;
            }

            return false;
        }
    }
}

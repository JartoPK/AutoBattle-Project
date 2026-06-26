using System;
using System.Collections.Generic;
using AutoBattle.Core.Units;
using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>
    /// Mundo 3D de la base (placeholder con primitivas). Edificios clicables
    /// (Reclutamiento, Mejoras) y las tropas del roster como cubos (coloreados por
    /// rareza) que deambulan por el suelo sin atravesar los edificios.
    /// </summary>
    public class BaseWorld
    {
        public readonly GameObject Root;

        private static readonly Vector3 CamPos = new(0f, 12f, -11f);
        private static readonly Quaternion CamRot = Quaternion.Euler(48f, 0f, 0f);

        private readonly Transform _troopsRoot;
        private readonly Dictionary<string, GameObject> _troops = new();
        private readonly List<Wanderer.Obstacle> _obstacles = new();

        public BaseWorld(Transform parent, Action onRecruitClicked, Action onUpgradesClicked)
        {
            Root = new GameObject("BaseWorld");
            Root.transform.SetParent(parent, false);

            WorldBuilder.Ground(Root.transform, "Ground", Vector3.zero, 34f, new Color(0.45f, 0.40f, 0.32f));

            // Edificios placeholder. Building() registra su huella como obstáculo.
            Building("Cuartel", new Vector3(-5.5f, 1f, 2f), new Vector3(3f, 2f, 3f), new Color(0.5f, 0.52f, 0.6f));
            Building("CasaHerencia", new Vector3(5.5f, 1f, 2f), new Vector3(3f, 2f, 3f), new Color(0.55f, 0.45f, 0.62f));

            var recruit = Building("Reclutamiento", new Vector3(0f, 1.1f, -2.5f), new Vector3(3.4f, 2.2f, 3.4f), new Color(0.92f, 0.62f, 0.18f));
            recruit.AddComponent<ClickableBuilding>().OnClicked = onRecruitClicked;
            WorldBuilder.Label3D(Root.transform, "RECLUTAMIENTO\n(toca el edificio)", new Vector3(0f, 2.9f, -2.5f));

            var upgrades = Building("Mejoras", new Vector3(0f, 1.1f, 5f), new Vector3(3.4f, 2.2f, 3.4f), new Color(0.25f, 0.5f, 0.75f));
            upgrades.AddComponent<ClickableBuilding>().OnClicked = onUpgradesClicked;
            WorldBuilder.Label3D(Root.transform, "MEJORAS\n(toca el edificio)", new Vector3(0f, 2.9f, 5f));

            _troopsRoot = new GameObject("Troops").transform;
            _troopsRoot.SetParent(Root.transform, false);
        }

        public void Show(Camera cam)
        {
            Root.SetActive(true);
            cam.transform.SetPositionAndRotation(CamPos, CamRot);
        }

        public void Hide() => Root.SetActive(false);

        /// <summary>Reconcilia los cubos de tropa con el roster (añade nuevas, quita las que ya no están).</summary>
        public void RefreshTroops(Roster roster)
        {
            var present = new HashSet<string>();
            foreach (var u in roster.units) present.Add(u.id);

            var gone = new List<string>();
            foreach (var kv in _troops)
                if (!present.Contains(kv.Key))
                {
                    UnityEngine.Object.Destroy(kv.Value);
                    gone.Add(kv.Key);
                }
            foreach (var id in gone) _troops.Remove(id);

            foreach (var u in roster.units)
            {
                if (_troops.ContainsKey(u.id)) continue;

                var cube = WorldBuilder.Box(_troopsRoot, $"Troop_{u.displayName}", SpawnPos(),
                    new Vector3(0.6f, 0.6f, 0.6f), RarityVisuals.Of(u.rarity));
                cube.AddComponent<Wanderer>().obstacles = _obstacles.ToArray();
                _troops[u.id] = cube;
            }
        }

        private GameObject Building(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = WorldBuilder.Box(Root.transform, name, pos, scale, color);
            _obstacles.Add(new Wanderer.Obstacle
            {
                center = new Vector2(pos.x, pos.z),
                radius = Mathf.Max(scale.x, scale.z) * 0.5f + 0.6f, // huella + margen del cubo
            });
            return go;
        }

        private Vector3 SpawnPos()
        {
            for (int i = 0; i < 18; i++)
            {
                var p = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0.3f, UnityEngine.Random.Range(-10f, 10f));
                if (!InObstacle(p)) return p;
            }
            return new Vector3(0f, 0.3f, 9f);
        }

        private bool InObstacle(Vector3 p)
        {
            var xz = new Vector2(p.x, p.z);
            foreach (var o in _obstacles)
                if (Vector2.Distance(xz, o.center) < o.radius) return true;
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using AutoBattle.Core.Units;
using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>
    /// Mundo 3D de la base (placeholder con primitivas). Varios edificios; el de
    /// Reclutamiento es clicable y abre el panel. Las tropas del roster se ven como
    /// cubos (coloreados por rareza) que deambulan por el suelo.
    /// </summary>
    public class BaseWorld
    {
        public readonly GameObject Root;

        private static readonly Vector3 CamPos = new(0f, 12f, -11f);
        private static readonly Quaternion CamRot = Quaternion.Euler(48f, 0f, 0f);

        private readonly Transform _troopsRoot;
        private readonly Dictionary<string, GameObject> _troops = new();

        public BaseWorld(Transform parent, Action onRecruitClicked)
        {
            Root = new GameObject("BaseWorld");
            Root.transform.SetParent(parent, false);

            WorldBuilder.Ground(Root.transform, "Ground", Vector3.zero, 34f, new Color(0.45f, 0.40f, 0.32f));

            // Edificios placeholder (futuras instalaciones).
            WorldBuilder.Box(Root.transform, "Cuartel", new Vector3(-5f, 1f, 1.5f), new Vector3(3f, 2f, 3f), new Color(0.5f, 0.52f, 0.6f));
            WorldBuilder.Box(Root.transform, "CasaHerencia", new Vector3(5f, 1f, 1.5f), new Vector3(3f, 2f, 3f), new Color(0.55f, 0.45f, 0.62f));

            // Edificio de Reclutamiento: clicable.
            var recruit = WorldBuilder.Box(Root.transform, "Reclutamiento", new Vector3(0f, 1.1f, -2f), new Vector3(3.4f, 2.2f, 3.4f), new Color(0.92f, 0.62f, 0.18f));
            recruit.AddComponent<ClickableBuilding>().OnClicked = onRecruitClicked;
            WorldBuilder.Label3D(Root.transform, "RECLUTAMIENTO\n(toca el edificio)", new Vector3(0f, 2.9f, -2f));

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

                var pos = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0.3f, UnityEngine.Random.Range(-10f, 10f));
                var cube = WorldBuilder.Box(_troopsRoot, $"Troop_{u.displayName}", pos,
                    new Vector3(0.6f, 0.6f, 0.6f), RarityVisuals.Of(u.rarity));
                cube.AddComponent<Wanderer>();
                _troops[u.id] = cube;
            }
        }
    }
}

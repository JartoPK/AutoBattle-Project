using System;
using AutoBattle.Meta.Campaign;
using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>
    /// Mundo 3D del mapa de conquista. Genera un marcador por nodo del CampaignMap
    /// (clicable) y permite desplazarse arrastrando. Cámara en vista isométrica.
    /// </summary>
    public class MapWorld
    {
        public readonly GameObject Root;

        private static readonly Vector3 CamPos = new(0f, 20f, -17f);
        private static readonly Quaternion CamRot = Quaternion.Euler(54f, 0f, 0f);

        private readonly MapPanHandler _pan;

        public MapWorld(Transform parent, CampaignMap map, Action<CampaignNodeData> onNodeClicked)
        {
            Root = new GameObject("MapWorld");
            Root.transform.SetParent(parent, false);

            WorldBuilder.Ground(Root.transform, "Sea", new Vector3(0f, -0.3f, 0f), 90f, new Color(0.16f, 0.40f, 0.55f));
            var island = WorldBuilder.Ground(Root.transform, "Island", Vector3.zero, 60f, new Color(0.35f, 0.62f, 0.30f));
            _pan = island.AddComponent<MapPanHandler>(); // arrastrar sobre la isla mueve la cámara

            if (map != null && map.nodes != null)
                foreach (var node in map.nodes)
                    CreateNodeMarker(node, onNodeClicked);
        }

        public void Show(Camera cam)
        {
            Root.SetActive(true);
            cam.transform.SetPositionAndRotation(CamPos, CamRot);
            _pan.cam = cam;
        }

        public void Hide() => Root.SetActive(false);

        private void CreateNodeMarker(CampaignNodeData node, Action<CampaignNodeData> onClicked)
        {
            var pos = new Vector3(node.mapPosition.x, 0.6f, node.mapPosition.y);
            var marker = WorldBuilder.Box(Root.transform, $"Node_{node.id}", pos,
                new Vector3(1.8f, 1.2f, 1.8f), NodeColor(node.type));
            marker.AddComponent<ClickableBuilding>().OnClicked = () => onClicked?.Invoke(node);

            WorldBuilder.Label3D(Root.transform, $"{node.displayName}\nDif {node.difficulty}",
                pos + new Vector3(0f, 1.6f, 0f));
        }

        private static Color NodeColor(NodeType type) => type switch
        {
            NodeType.Combate => new Color(0.85f, 0.80f, 0.5f),
            NodeType.Elite => new Color(0.9f, 0.55f, 0.2f),
            NodeType.Jefe => new Color(0.85f, 0.3f, 0.3f),
            NodeType.Reclutamiento => new Color(0.4f, 0.6f, 0.9f),
            NodeType.Recursos => new Color(0.9f, 0.85f, 0.3f),
            _ => Color.white,
        };
    }
}

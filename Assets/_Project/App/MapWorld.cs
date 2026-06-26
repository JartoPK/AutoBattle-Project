using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>
    /// Mundo 3D del mapa (placeholder con primitivas). Los nodos de conquista
    /// reales llegan en la Fase 7. Coloca la cámara en una vista isométrica.
    /// </summary>
    public class MapWorld
    {
        public readonly GameObject Root;

        private static readonly Vector3 CamPos = new(0f, 15f, -12f);
        private static readonly Quaternion CamRot = Quaternion.Euler(52f, 0f, 0f);

        public MapWorld(Transform parent)
        {
            Root = new GameObject("MapWorld");
            Root.transform.SetParent(parent, false);

            WorldBuilder.Ground(Root.transform, "Sea", new Vector3(0f, -0.25f, 0f), 70f, new Color(0.16f, 0.40f, 0.55f));
            WorldBuilder.Ground(Root.transform, "Island", Vector3.zero, 26f, new Color(0.35f, 0.62f, 0.30f));

            // Nodos de ejemplo.
            WorldBuilder.Box(Root.transform, "Nodo1", new Vector3(-5f, 0.5f, 2f), new Vector3(1.6f, 1f, 1.6f), new Color(0.9f, 0.82f, 0.5f));
            WorldBuilder.Box(Root.transform, "Nodo2", new Vector3(0f, 0.5f, -1f), new Vector3(1.6f, 1f, 1.6f), new Color(0.9f, 0.82f, 0.5f));
            WorldBuilder.Box(Root.transform, "NodoJefe", new Vector3(5f, 0.7f, 3f), new Vector3(1.8f, 1.4f, 1.8f), new Color(0.8f, 0.45f, 0.4f));
            WorldBuilder.Label3D(Root.transform, "Expediciones (Fase 7)", new Vector3(0f, 2.4f, 4f));
        }

        public void Show(Camera cam)
        {
            Root.SetActive(true);
            cam.transform.SetPositionAndRotation(CamPos, CamRot);
        }

        public void Hide() => Root.SetActive(false);
    }
}

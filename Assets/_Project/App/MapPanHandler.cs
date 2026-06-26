using UnityEngine;
using UnityEngine.EventSystems;

namespace AutoBattle.App
{
    /// <summary>
    /// Arrastrar sobre el suelo del mapa desplaza la cámara por el plano (X/Z),
    /// dentro de unos límites. Como es un IDragHandler, no interfiere con el clic
    /// en los nodos (un clic sin arrastre llega al nodo).
    /// </summary>
    public class MapPanHandler : MonoBehaviour, IDragHandler
    {
        public Camera cam;
        public float panSpeed = 0.035f;
        public float limitX = 22f;
        public float limitZ = 22f;

        public void OnDrag(PointerEventData eventData)
        {
            if (cam == null) return;

            var move = new Vector3(-eventData.delta.x, 0f, -eventData.delta.y) * panSpeed;
            var p = cam.transform.position + move;
            p.x = Mathf.Clamp(p.x, -limitX, limitX);
            p.z = Mathf.Clamp(p.z, -limitZ, limitZ);
            cam.transform.position = p;
        }
    }
}

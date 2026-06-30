using UnityEngine;
using UnityEngine.EventSystems;

namespace AutoBattle.App
{
    public class TreeZoomPan : MonoBehaviour, IDragHandler, IScrollHandler
    {
        public RectTransform content;
        public float zoomSpeed = 0.1f;
        public float minZoom = 0.3f;
        public float maxZoom = 2f;

        private float _zoom = 1f;

        public void OnDrag(PointerEventData eventData)
        {
            if (content == null) return;
            content.anchoredPosition += eventData.delta / _zoom;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (content == null) return;
            float prev = _zoom;
            _zoom = Mathf.Clamp(_zoom + eventData.scrollDelta.y * zoomSpeed, minZoom, maxZoom);
            content.localScale = Vector3.one * _zoom;

            // Zoom toward mouse position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                content.parent as RectTransform, eventData.position, eventData.pressEventCamera, out var local);
            float factor = 1f - prev / _zoom;
            content.anchoredPosition += (content.anchoredPosition - local) * factor;
        }
    }
}

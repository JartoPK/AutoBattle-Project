using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AutoBattle.App
{
    /// <summary>
    /// Hace clicable un objeto 3D con collider. Funciona gracias al PhysicsRaycaster
    /// de la cámara + el EventSystem (Input System). Invoca <see cref="OnClicked"/>.
    /// </summary>
    public class ClickableBuilding : MonoBehaviour, IPointerClickHandler
    {
        public Action OnClicked;

        public void OnPointerClick(PointerEventData eventData) => OnClicked?.Invoke();
    }
}

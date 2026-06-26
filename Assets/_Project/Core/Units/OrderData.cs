using UnityEngine;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Definición de una Orden de Unidad (acción táctica que se carga en una
    /// ranura antes del combate). La Meta la usa para el deployment (qué órdenes
    /// puede equipar cada clase); el COMPORTAMIENTO lo implementa Combat (Fase 3).
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Order Data", fileName = "Order")]
    public class OrderData : ScriptableObject
    {
        [Tooltip("Identificador estable. Se guarda en el loadout de la unidad.")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
    }
}

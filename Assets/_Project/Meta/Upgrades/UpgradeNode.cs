using UnityEngine;

namespace AutoBattle.Meta.Upgrades
{
    /// <summary>Un nodo del árbol de mejoras: coste, requisitos y un efecto.</summary>
    [CreateAssetMenu(menuName = "AutoBattle/Upgrade Node", fileName = "Upgrade")]
    public class UpgradeNode : ScriptableObject
    {
        [Tooltip("Identificador estable. Se guarda en la partida; NO cambiarlo.")]
        public string id;
        public string displayName;
        [TextArea] public string description;

        public UpgradeBranch branch;

        [Header("Requisitos")]
        public int cost;
        [Tooltip("Nivel de base mínimo para poder comprarlo.")]
        public int requiredBaseLevel = 1;
        [Tooltip("Ids de nodos que deben estar desbloqueados antes.")]
        public string[] prerequisiteIds;

        [Header("Efecto")]
        public UpgradeEffectType effect;
        public float effectValue;
        [Tooltip("Para flags de combate: id de lo que desbloquea (orden, pasiva...).")]
        public string effectTargetId;
    }
}

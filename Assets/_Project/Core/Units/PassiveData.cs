using UnityEngine;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Definición de una pasiva (rasgo siempre activo). Una unidad nace con una,
    /// elegida al azar del pool de su clase.
    /// El EFECTO lo implementa la capa Combat leyendo <see cref="id"/>; aquí solo
    /// vive el dato compartido (nombre, descripción, icono).
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Passive Data", fileName = "Passive")]
    public class PassiveData : ScriptableObject
    {
        [Tooltip("Identificador estable. Se guarda en cada unidad; NO cambiarlo tras crear partidas.")]
        public string id;
        public string displayName;

        [Tooltip("Calidad de la pasiva. Una unidad solo puede tener pasivas de rareza <= la suya.")]
        public Rarity rarity;

        [TextArea] public string description;
        public Sprite icon;
    }
}

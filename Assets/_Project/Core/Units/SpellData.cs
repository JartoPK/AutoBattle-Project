using UnityEngine;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Definición de un hechizo de mago. La lógica de lanzamiento y maná vive en
    /// Combat (Fase 5); aquí solo el dato compartido (incluido el coste, que la
    /// Meta puede mostrar en menús).
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Spell Data", fileName = "Spell")]
    public class SpellData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public float manaCost;
        public Sprite icon;
    }
}

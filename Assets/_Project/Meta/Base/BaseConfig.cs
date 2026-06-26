using UnityEngine;

namespace AutoBattle.Meta.Base
{
    /// <summary>
    /// Parámetros de la base que dependen de su nivel. "La base como
    /// multiplicador": subir nivel agranda el tope de roster y mejora la herencia.
    /// El nivel de base se sube vía efectos del árbol de mejoras (Paso 8B).
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Base Config", fileName = "BaseConfig")]
    public class BaseConfig : ScriptableObject
    {
        [Header("Tope de roster (crece con el nivel de base)")]
        public int baseRosterCap = 8;
        public int rosterCapPerLevel = 2;

        [Header("Herencia")]
        [Range(0f, 1f)]
        [Tooltip("Fracción de las stats del veterano que hereda la recluta.")]
        public float statInheritPercent = 0.6f;

        /// <summary>Tope de roster para un nivel de base dado (nivel 1 = baseRosterCap).</summary>
        public int GetRosterCap(int baseLevel) =>
            baseRosterCap + rosterCapPerLevel * Mathf.Max(0, baseLevel - 1);
    }
}

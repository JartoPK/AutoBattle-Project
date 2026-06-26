using UnityEngine;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Baremo UNIVERSAL del que se tiran las stats base de cualquier unidad,
    /// independientemente de su clase (decisión de diseño: "ADN puro").
    /// Si en el futuro se quieren modificadores por clase, se añadirán como un
    /// paso aparte sobre estas tiradas, sin tocar este baremo.
    /// Valores iniciales orientativos; ajustar en balance.
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Unit Generation Config", fileName = "UnitGenerationConfig")]
    public class UnitGenerationConfig : ScriptableObject
    {
        [Header("Baremo universal (igual para todas las clases)")]
        public StatRange hp = new StatRange(80f, 120f);
        public StatRange attack = new StatRange(8f, 14f);

        [Tooltip("Ataques por segundo.")]
        public StatRange attackSpeed = new StatRange(0.8f, 1.4f);

        public StatRange moveSpeed = new StatRange(2.5f, 4.0f);

        [Header("Solo se aplica a clases con usesMana = true")]
        public StatRange mana = new StatRange(80f, 120f);
    }
}

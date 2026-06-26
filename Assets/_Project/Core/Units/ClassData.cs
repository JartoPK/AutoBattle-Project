using UnityEngine;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// "Molde" de clase: define arma/rol y los pools de pasivas, órdenes y
    /// hechizos que una unidad de esta clase PUEDE tener. No contiene las stats
    /// (esas salen del baremo universal en <see cref="UnitGenerationConfig"/>),
    /// salvo el flag de maná.
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Class Data", fileName = "Class")]
    public class ClassData : ScriptableObject
    {
        public UnitClass classId;
        public string displayName;

        [Tooltip("Prefab visual y de combate. Lo provee la capa Combat; la Meta solo lo referencia.")]
        public GameObject unitPrefab;

        [Tooltip("Si está activo, la unidad tira también la stat de maná.")]
        public bool usesMana;

        [Header("Pools (tiradas POSIBLES, no garantizadas)")]
        [Tooltip("De aquí se elige al azar la única pasiva con la que nace la unidad.")]
        public PassiveData[] possiblePassives;

        [Tooltip("Órdenes que esta clase puede cargar en sus ranuras durante el deployment.")]
        public OrderData[] availableOrders;

        [Tooltip("Hechizos disponibles para esta clase (solo magos).")]
        public SpellData[] spells;
    }
}

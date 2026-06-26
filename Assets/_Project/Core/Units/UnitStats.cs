using System;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Estadísticas concretas de una unidad (tronco común a todas las clases).
    /// Struct serializable para que el SaveSystem la persista dentro de UnitInstance.
    /// </summary>
    [Serializable]
    public struct UnitStats
    {
        public float hp;
        public float attack;        // ATQ: daño por golpe (sin armadura en MVP).
        public float attackSpeed;   // Ataques por segundo (intervalo = 1 / attackSpeed).
        public float moveSpeed;
        public float mana;          // 0 si la clase no usa maná.
    }
}

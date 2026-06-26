using System;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Una unidad concreta del roster: su "ADN" persistente. Serializable para el
    /// SaveSystem. Referencia a clase y pasiva por id (no por objeto) para
    /// sobrevivir a guardado/carga; se resuelven con <see cref="ClassDatabase"/>.
    ///
    /// Es el objeto que la Meta entrega al combate. El combate LEE estas stats
    /// para instanciar la unidad y, al terminar, devuelve un resultado (quién
    /// murió, HP mínimo) que la Meta aplica.
    /// </summary>
    [Serializable]
    public class UnitInstance
    {
        public string id;            // GUID único, estable de por vida.
        public string displayName;
        public UnitClass classId;
        public Rarity rarity;        // Calidad con la que se generó (afecta a sus stats).
        public UnitStats baseStats;  // ADN base. Solo lo modifican cicatrices (Fase 6).
        public string passiveId;     // id de la PassiveData con la que nació.
        public int battlesSurvived;

        public bool IsVeteran => battlesSurvived > 0;

        /// <summary>
        /// Punto ÚNICO donde en el futuro se aplicarán modificadores de clase o
        /// cicatrices. Hoy las stats son ADN puro, así que devuelve las base tal cual.
        /// El combate debería consumir SIEMPRE esto, no <see cref="baseStats"/>.
        /// </summary>
        public UnitStats EffectiveStats => baseStats;
    }
}

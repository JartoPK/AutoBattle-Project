using System;
using System.Collections.Generic;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Creación de unidades. Una recluta nace SIN clase (solo stats); el jugador
    /// le asigna la clase después, lo que rola su pasiva del pool de esa clase.
    /// </summary>
    public static class UnitFactory
    {
        /// <summary>Crea una recluta sin clase: solo tira las stats base (sin maná ni pasiva).</summary>
        public static UnitInstance CreateClassless(UnitGenerationConfig config,
            Rarity rarity, RarityConfig rarityConfig, Random rng = null, float qualityBonus = 0f)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            rng ??= new Random();

            var stats = new UnitStats
            {
                hp = Roll(config.hp, rarity, rarityConfig, rng, qualityBonus),
                attack = Roll(config.attack, rarity, rarityConfig, rng, qualityBonus),
                attackSpeed = Roll(config.attackSpeed, rarity, rarityConfig, rng, qualityBonus),
                moveSpeed = Roll(config.moveSpeed, rarity, rarityConfig, rng, qualityBonus),
                mana = 0f, // se rola al asignar una clase con maná.
            };

            return new UnitInstance
            {
                id = Guid.NewGuid().ToString("N"),
                displayName = NameGenerator.Next(rng),
                hasClass = false,
                rarity = rarity,
                baseStats = stats,
                passiveId = string.Empty,
                battlesSurvived = 0,
            };
        }

        /// <summary>Asigna una clase a una unidad: fija la clase, rola el maná (si aplica) y la pasiva.</summary>
        public static void AssignClass(UnitInstance unit, ClassData classData,
            UnitGenerationConfig config, RarityConfig rarityConfig, Random rng = null)
        {
            if (unit == null || classData == null || config == null) return;
            rng ??= new Random();

            unit.classId = classData.classId;
            unit.hasClass = true;

            if (classData.usesMana && unit.baseStats.mana <= 0f)
                unit.baseStats.mana = Roll(config.mana, unit.rarity, rarityConfig, rng);

            var passive = PickPassive(classData, unit.rarity, rng);
            unit.passiveId = passive != null ? passive.id : string.Empty;
        }

        /// <summary>Crea una unidad ya con clase (recluta sin clase + asignación en un paso).</summary>
        public static UnitInstance Create(ClassData classData, UnitGenerationConfig config,
            Rarity rarity, RarityConfig rarityConfig, Random rng = null, float qualityBonus = 0f)
        {
            if (classData == null) throw new ArgumentNullException(nameof(classData));
            var unit = CreateClassless(config, rarity, rarityConfig, rng, qualityBonus);
            AssignClass(unit, classData, config, rarityConfig, rng);
            return unit;
        }

        /// <summary>Atajo: unidad común con clase, tirando de todo el baremo.</summary>
        public static UnitInstance Create(ClassData classData, UnitGenerationConfig config, Random rng = null)
            => Create(classData, config, Rarity.Comun, null, rng);

        // Tira una stat: percentil (según rareza + bonus) convertido a valor real del baremo.
        private static float Roll(StatRange baremo, Rarity rarity, RarityConfig cfg, Random rng, float qualityBonus = 0f)
        {
            var window = cfg != null ? cfg.GetPercentileWindow(rarity) : new StatRange(0f, 1f);
            if (qualityBonus != 0f)
                window = new StatRange(Clamp01(window.min + qualityBonus), Clamp01(window.max + qualityBonus));

            float percentile = window.Roll(rng);
            return baremo.min + percentile * (baremo.max - baremo.min);
        }

        // Elige una pasiva del pool cuya rareza sea <= la de la unidad.
        private static PassiveData PickPassive(ClassData classData, Rarity maxRarity, Random rng)
        {
            var pool = classData.possiblePassives;
            if (pool == null || pool.Length == 0) return null;

            var eligible = new List<PassiveData>();
            foreach (var p in pool)
                if (p != null && p.rarity <= maxRarity) eligible.Add(p);

            if (eligible.Count > 0) return eligible[rng.Next(eligible.Count)];

            PassiveData lowest = null;
            foreach (var p in pool)
                if (p != null && (lowest == null || p.rarity < lowest.rarity)) lowest = p;
            return lowest;
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}

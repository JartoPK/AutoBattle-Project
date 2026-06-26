using System;
using System.Collections.Generic;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Crea unidades nuevas: tira las stats base del baremo universal (moduladas
    /// por la rareza) y elige al azar una pasiva del pool de la clase compatible
    /// con esa rareza. Único lugar donde "nace" una unidad.
    /// </summary>
    public static class UnitFactory
    {
        /// <summary>Crea una unidad de la rareza indicada.</summary>
        /// <param name="rarityConfig">Si es null, se tira de todo el baremo (sin sesgo de rareza).</param>
        /// <param name="rng">RNG opcional para reproducibilidad; si es null se usa uno nuevo.</param>
        /// <param name="qualityBonus">Desplaza la ventana de percentil hacia arriba [0..1] (bonus del árbol de mejoras).</param>
        public static UnitInstance Create(ClassData classData, UnitGenerationConfig config,
            Rarity rarity, RarityConfig rarityConfig, Random rng = null, float qualityBonus = 0f)
        {
            if (classData == null) throw new ArgumentNullException(nameof(classData));
            if (config == null) throw new ArgumentNullException(nameof(config));
            rng ??= new Random();

            var window = rarityConfig != null
                ? rarityConfig.GetPercentileWindow(rarity)
                : new StatRange(0f, 1f);

            if (qualityBonus != 0f)
                window = new StatRange(Clamp01(window.min + qualityBonus), Clamp01(window.max + qualityBonus));

            var stats = new UnitStats
            {
                hp = RollStat(config.hp, window, rng),
                attack = RollStat(config.attack, window, rng),
                attackSpeed = RollStat(config.attackSpeed, window, rng),
                moveSpeed = RollStat(config.moveSpeed, window, rng),
                mana = classData.usesMana ? RollStat(config.mana, window, rng) : 0f,
            };

            var passive = PickPassive(classData, rarity, rng);

            return new UnitInstance
            {
                id = Guid.NewGuid().ToString("N"),
                displayName = NameGenerator.Next(rng),
                classId = classData.classId,
                rarity = rarity,
                baseStats = stats,
                passiveId = passive != null ? passive.id : string.Empty,
                battlesSurvived = 0,
            };
        }

        /// <summary>Atajo: unidad común tirando de todo el baremo (sin sesgo).</summary>
        public static UnitInstance Create(ClassData classData, UnitGenerationConfig config, Random rng = null)
            => Create(classData, config, Rarity.Comun, null, rng);

        // Convierte un percentil [0..1] en un valor real dentro del baremo de la stat.
        private static float RollStat(StatRange baremo, StatRange percentileWindow, Random rng)
        {
            float percentile = percentileWindow.Roll(rng);
            return baremo.min + percentile * (baremo.max - baremo.min);
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        // Elige una pasiva del pool cuya rareza sea <= la de la unidad.
        private static PassiveData PickPassive(ClassData classData, Rarity maxRarity, Random rng)
        {
            var pool = classData.possiblePassives;
            if (pool == null || pool.Length == 0) return null;

            var eligible = new List<PassiveData>();
            foreach (var p in pool)
                if (p != null && p.rarity <= maxRarity) eligible.Add(p);

            if (eligible.Count > 0) return eligible[rng.Next(eligible.Count)];

            // Fallback: ninguna compatible -> la de menor rareza disponible.
            PassiveData lowest = null;
            foreach (var p in pool)
                if (p != null && (lowest == null || p.rarity < lowest.rarity)) lowest = p;
            return lowest;
        }
    }
}

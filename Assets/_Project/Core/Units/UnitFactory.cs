using System;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Crea unidades nuevas: tira las stats base del baremo universal y elige al
    /// azar una pasiva del pool de la clase. Único lugar donde "nace" una unidad.
    /// </summary>
    public static class UnitFactory
    {
        /// <param name="rng">RNG opcional para reproducibilidad; si es null se usa uno nuevo.</param>
        public static UnitInstance Create(ClassData classData, UnitGenerationConfig config, Random rng = null)
        {
            if (classData == null) throw new ArgumentNullException(nameof(classData));
            if (config == null) throw new ArgumentNullException(nameof(config));
            rng ??= new Random();

            var stats = new UnitStats
            {
                hp = config.hp.Roll(rng),
                attack = config.attack.Roll(rng),
                attackSpeed = config.attackSpeed.Roll(rng),
                moveSpeed = config.moveSpeed.Roll(rng),
                mana = classData.usesMana ? config.mana.Roll(rng) : 0f,
            };

            var passive = PickPassive(classData, rng);

            return new UnitInstance
            {
                id = Guid.NewGuid().ToString("N"),
                displayName = NameGenerator.Next(rng),
                classId = classData.classId,
                baseStats = stats,
                passiveId = passive != null ? passive.id : string.Empty,
                battlesSurvived = 0,
            };
        }

        private static PassiveData PickPassive(ClassData classData, Random rng)
        {
            var pool = classData.possiblePassives;
            if (pool == null || pool.Length == 0) return null;
            return pool[rng.Next(pool.Length)];
        }
    }
}

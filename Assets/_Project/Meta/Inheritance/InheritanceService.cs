using System;
using AutoBattle.Core.Units;
using AutoBattle.Meta.Base;
using AutoBattle.Meta.Recruitment;

namespace AutoBattle.Meta.Inheritance
{
    /// <summary>
    /// Casa de la herencia: consume un veterano y genera una recluta que hereda
    /// su pasiva y un porcentaje de sus stats. La CLASE la elige el jugador (única
    /// excepción a "clase permanente").
    ///
    /// Nota de diseño: la pasiva heredada puede quedar "cross-class" si se cambia
    /// la clase (la pasiva venía del pool de la clase original). Aceptado a
    /// propósito como rasgo heredado; revisar si se quiere restringir.
    /// </summary>
    public static class InheritanceService
    {
        /// <returns>La recluta resultante, o null si el veterano/clase no son válidos.</returns>
        public static UnitInstance Inherit(MetaGameState state, string veteranId, UnitClass chosenClass,
            RecruitmentConfig config, BaseConfig baseConfig, Random rng = null)
        {
            if (state == null || config == null || baseConfig == null
                || config.classDatabase == null || config.generationConfig == null)
                return null;

            var veteran = state.roster.Get(veteranId);
            if (veteran == null) return null;

            var classData = config.classDatabase.Get(chosenClass);
            if (classData == null) return null;

            rng ??= new Random();
            float pct = baseConfig.statInheritPercent;
            var v = veteran.baseStats;

            var stats = new UnitStats
            {
                hp = v.hp * pct,
                attack = v.attack * pct,
                attackSpeed = v.attackSpeed * pct,
                moveSpeed = v.moveSpeed * pct,
                // El maná depende de la nueva clase: si la clase usa maná pero el
                // veterano no tenía (cambio de clase), se tira fresco y se hereda %.
                mana = classData.usesMana
                    ? (v.mana > 0f ? v.mana * pct : config.generationConfig.mana.Roll(rng) * pct)
                    : 0f,
            };

            var recruit = new UnitInstance
            {
                id = Guid.NewGuid().ToString("N"),
                displayName = NameGenerator.Next(rng),
                classId = chosenClass,
                rarity = veteran.rarity,
                baseStats = stats,
                passiveId = veteran.passiveId,
                battlesSurvived = 0,
            };

            state.roster.Remove(veteranId); // el veterano se consume
            state.roster.Add(recruit);
            return recruit;
        }
    }
}

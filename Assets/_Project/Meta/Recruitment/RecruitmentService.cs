using System;
using System.Collections.Generic;
using AutoBattle.Core.Units;
using AutoBattle.Meta.Base;

namespace AutoBattle.Meta.Recruitment
{
    public enum RecruitFailReason { None, RosterLleno, SinMonedas, ConfigInvalida }

    public struct RecruitResult
    {
        public bool success;
        public UnitInstance unit;
        public RecruitFailReason reason;

        public static RecruitResult Ok(UnitInstance unit) => new() { success = true, unit = unit };
        public static RecruitResult Fail(RecruitFailReason reason) => new() { success = false, reason = reason };
    }

    /// <summary>
    /// Recluta una unidad de un tier: comprueba tope de roster y monedas, tira la
    /// rareza en la ruleta del tier, elige clase al azar y genera la unidad.
    /// Toda la lógica opera sobre el <see cref="MetaGameState"/> recibido.
    /// </summary>
    public static class RecruitmentService
    {
        public static RecruitResult Recruit(MetaGameState state, RecruitmentConfig.RecruitTier tier,
            RecruitmentConfig config, BaseConfig baseConfig, Random rng = null)
        {
            if (state == null || tier == null || config == null || baseConfig == null
                || config.generationConfig == null || config.classDatabase == null)
                return RecruitResult.Fail(RecruitFailReason.ConfigInvalida);

            rng ??= new Random();

            int cap = baseConfig.GetRosterCap(state.baseLevel);
            if (state.roster.Count >= cap) return RecruitResult.Fail(RecruitFailReason.RosterLleno);
            if (!state.wallet.CanAfford(tier.cost)) return RecruitResult.Fail(RecruitFailReason.SinMonedas);

            var classData = PickRandomClass(config.classDatabase, rng);
            if (classData == null) return RecruitResult.Fail(RecruitFailReason.ConfigInvalida);

            var rarity = DrawRarity(tier, rng);
            var unit = UnitFactory.Create(classData, config.generationConfig, rarity, config.rarityConfig, rng);

            state.wallet.Spend(tier.cost);
            state.roster.Add(unit);
            return RecruitResult.Ok(unit);
        }

        // Ruleta ponderada sobre las rarezas del tier.
        private static Rarity DrawRarity(RecruitmentConfig.RecruitTier tier, Random rng)
        {
            var table = tier.rarityTable;
            if (table == null || table.Length == 0) return Rarity.Comun;

            float total = 0f;
            foreach (var rw in table) total += Math.Max(0f, rw.weight);
            if (total <= 0f) return table[0].rarity;

            double roll = rng.NextDouble() * total;
            float acc = 0f;
            foreach (var rw in table)
            {
                acc += Math.Max(0f, rw.weight);
                if (roll <= acc) return rw.rarity;
            }
            return table[table.Length - 1].rarity;
        }

        private static ClassData PickRandomClass(ClassDatabase db, Random rng)
        {
            var classes = db.classes;
            if (classes == null || classes.Length == 0) return null;

            var valid = new List<ClassData>();
            foreach (var c in classes)
                if (c != null) valid.Add(c);

            return valid.Count == 0 ? null : valid[rng.Next(valid.Count)];
        }
    }
}

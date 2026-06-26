using System;
using AutoBattle.Core.Units;

namespace AutoBattle.Meta.Recruitment
{
    /// <summary>
    /// Asigna la clase (elegida por el jugador) a una tropa del roster que aún no
    /// la tiene. Al asignarla, se rola su pasiva del pool de esa clase y, si la
    /// clase usa maná, su maná. No se puede reasignar una clase ya fijada (la
    /// clase es permanente; la reasignación con coste queda para el futuro).
    /// </summary>
    public static class ClassAssignmentService
    {
        public static bool Assign(MetaGameState state, string unitId, UnitClass chosenClass,
            RecruitmentConfig config, Random rng = null)
        {
            if (state == null || config == null || config.classDatabase == null || config.generationConfig == null)
                return false;

            var unit = state.roster.Get(unitId);
            if (unit == null || unit.hasClass) return false;

            var classData = config.classDatabase.Get(chosenClass);
            if (classData == null) return false;

            UnitFactory.AssignClass(unit, classData, config.generationConfig, config.rarityConfig, rng);
            return true;
        }
    }
}

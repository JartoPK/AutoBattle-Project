using System;
using AutoBattle.Core.Units;
using AutoBattle.Meta.Base;
using AutoBattle.Meta.Economy;
using AutoBattle.Meta.Upgrades;
using UnityEngine;

namespace AutoBattle.Meta
{
    /// <summary>
    /// Estado dinámico persistente de la partida (capa meta). Es el objeto que el
    /// SaveSystem guarda/carga. La progresión de campaña (Fase 7) se añadirá aquí.
    /// </summary>
    [Serializable]
    public class MetaGameState
    {
        public Roster roster = new();
        public Wallet wallet = new();
        public int baseLevel = 1;

        [Header("Árbol de mejoras")]
        public UpgradeState upgrades = new();

        // Modificadores acumulados por el árbol (los aplica UpgradeService).
        public int upgradeBonusRosterCap;
        public float recruitQualityBonus; // desplazamiento de percentil [0..1] al reclutar.
        public float inheritBonus;        // se suma a statInheritPercent.

        /// <summary>Tope de roster efectivo: base por nivel + bonus del árbol.</summary>
        public int GetRosterCap(BaseConfig config) =>
            config.GetRosterCap(baseLevel) + upgradeBonusRosterCap;

        /// <summary>% de herencia efectivo: base + bonus del árbol (acotado a 1).</summary>
        public float GetInheritPercent(BaseConfig config) =>
            Mathf.Clamp01(config.statInheritPercent + inheritBonus);

        public static MetaGameState New(int startingCoins = 0) => new()
        {
            roster = new Roster(),
            wallet = new Wallet(startingCoins),
            baseLevel = 1,
            upgrades = new UpgradeState(),
        };
    }
}

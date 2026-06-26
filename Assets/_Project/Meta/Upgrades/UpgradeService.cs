using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoBattle.Meta.Upgrades
{
    public enum UpgradeFailReason
    {
        None, ConfigInvalida, YaDesbloqueado, NivelBaseInsuficiente, PrerequisitoFaltante, SinMonedas
    }

    public struct UpgradePurchaseResult
    {
        public bool success;
        public UpgradeFailReason reason;

        public static UpgradePurchaseResult Ok() => new() { success = true };
        public static UpgradePurchaseResult Fail(UpgradeFailReason r) => new() { success = false, reason = r };
    }

    /// <summary>
    /// Compra de nodos del árbol: valida requisitos (desbloqueado, nivel de base,
    /// prerequisitos, monedas), descuenta, registra y aplica el efecto.
    /// Los efectos meta mutan el MetaGameState; los flags de combate solo quedan
    /// registrados para que la capa Combat los consulte.
    /// </summary>
    public static class UpgradeService
    {
        public static UpgradePurchaseResult CanPurchase(MetaGameState state, UpgradeNode node)
        {
            if (state == null || node == null) return UpgradePurchaseResult.Fail(UpgradeFailReason.ConfigInvalida);
            if (state.upgrades.IsUnlocked(node.id)) return UpgradePurchaseResult.Fail(UpgradeFailReason.YaDesbloqueado);
            if (state.baseLevel < node.requiredBaseLevel) return UpgradePurchaseResult.Fail(UpgradeFailReason.NivelBaseInsuficiente);

            if (node.prerequisiteIds != null)
                foreach (var pre in node.prerequisiteIds)
                    if (!string.IsNullOrEmpty(pre) && !state.upgrades.IsUnlocked(pre))
                        return UpgradePurchaseResult.Fail(UpgradeFailReason.PrerequisitoFaltante);

            if (!state.wallet.CanAfford(node.cost)) return UpgradePurchaseResult.Fail(UpgradeFailReason.SinMonedas);
            return UpgradePurchaseResult.Ok();
        }

        public static UpgradePurchaseResult Purchase(MetaGameState state, UpgradeNode node)
        {
            var check = CanPurchase(state, node);
            if (!check.success) return check;

            state.wallet.Spend(node.cost);
            state.upgrades.Unlock(node.id);
            ApplyEffect(state, node);
            return UpgradePurchaseResult.Ok();
        }

        private static void ApplyEffect(MetaGameState state, UpgradeNode node)
        {
            switch (node.effect)
            {
                case UpgradeEffectType.IncreaseBaseLevel:
                    state.baseLevel += Mathf.Max(1, Mathf.RoundToInt(node.effectValue));
                    break;
                case UpgradeEffectType.IncreaseRosterCap:
                    state.upgradeBonusRosterCap += Mathf.RoundToInt(node.effectValue);
                    break;
                case UpgradeEffectType.RecruitQualityBonus:
                    state.recruitQualityBonus += node.effectValue;
                    break;
                case UpgradeEffectType.InheritanceQualityBonus:
                    state.inheritBonus += node.effectValue;
                    break;
                // Flags de combate: no mutan el estado meta; basta con que el nodo
                // quede en la lista de desbloqueados (lo consulta la capa Combat).
            }
        }

        // ---- Consultas para la capa Combat ----

        /// <summary>¿Hay algún nodo desbloqueado con este tipo de efecto? (p.ej. 3.ª ranura).</summary>
        public static bool IsFlagUnlocked(MetaGameState state, UpgradeTree tree, UpgradeEffectType effect)
        {
            foreach (var id in state.upgrades.unlockedNodeIds)
            {
                var n = tree.Get(id);
                if (n != null && n.effect == effect) return true;
            }
            return false;
        }

        /// <summary>Ids objetivo (órdenes, pasivas...) desbloqueados para un tipo de efecto.</summary>
        public static List<string> GetUnlockedTargets(MetaGameState state, UpgradeTree tree, UpgradeEffectType effect)
        {
            var result = new List<string>();
            foreach (var id in state.upgrades.unlockedNodeIds)
            {
                var n = tree.Get(id);
                if (n != null && n.effect == effect && !string.IsNullOrEmpty(n.effectTargetId))
                    result.Add(n.effectTargetId);
            }
            return result;
        }
    }
}

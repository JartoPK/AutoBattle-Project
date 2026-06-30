using UnityEditor;
using UnityEngine;
using AutoBattle.Meta.Upgrades;

namespace AutoBattle.App.Editor
{
    public static class UpgradeTreeSetup
    {
        private const string DataPath = "Assets/_Project/Data/Upgrades";

        [MenuItem("AutoBattle/Setup/Generar árbol de mejoras (ejemplo)")]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data"))
                AssetDatabase.CreateFolder("Assets/_Project", "Data");
            if (!AssetDatabase.IsValidFolder(DataPath))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Upgrades");

            //  Layout (Y positive = up):
            //
            //                cmd_start (0, 300)
            //              /     |       \       \
            //     cmd_roster  cmd_recruits  war_power  arc_precision  mage_power
            //      (-300,100) (-100,100)   (100,100)    (300,100)     (500,100)
            //          \       /             |    \        |             |
            //       cmd_base2           war_pass war_ord arc_speed   mage_pass
            //       (-200,-100)         (50,-100)(200,-100)(300,-100)(500,-100)
            //        /       \              |                |          |
            //   cmd_inherit cmd_3rd    war_statcap      arc_order   mage_order
            //  (-300,-300) (-100,-300) (50,-300)       (300,-300)  (500,-300)

            var nodes = new[]
            {
                // ── Root ──
                N("cmd_start", "Puesto de mando", "Desbloquea el árbol.",
                    UpgradeBranch.Comandante, 0, UpgradeEffectType.IncreaseBaseLevel, 1f,
                    1, new string[0], new Vector2(0, 300)),

                // ── Comandante tier 1 ──
                N("cmd_roster", "Barracones", "+2 roster.",
                    UpgradeBranch.Comandante, 0, UpgradeEffectType.IncreaseRosterCap, 2f,
                    1, new[] { "cmd_start" }, new Vector2(-300, 100)),

                N("cmd_recruits", "Reclutas elite", "Mejores stats al reclutar.",
                    UpgradeBranch.Comandante, 0, UpgradeEffectType.RecruitQualityBonus, 0.1f,
                    1, new[] { "cmd_start" }, new Vector2(-100, 100)),

                // ── Comandante tier 2 ──
                N("cmd_base2", "Ampliar base", "Sube nivel de base.",
                    UpgradeBranch.Comandante, 0, UpgradeEffectType.IncreaseBaseLevel, 1f,
                    1, new[] { "cmd_roster", "cmd_recruits" }, new Vector2(-200, -100)),

                // ── Comandante tier 3 ──
                N("cmd_inherit", "Herencia+", "Tropas heredan mejores stats.",
                    UpgradeBranch.Comandante, 0, UpgradeEffectType.InheritanceQualityBonus, 0.15f,
                    2, new[] { "cmd_base2" }, new Vector2(-300, -300)),

                N("cmd_third_slot", "3ª ranura", "Añade 3ª ranura de órdenes.",
                    UpgradeBranch.Comandante, 0, UpgradeEffectType.UnlockThirdOrderSlot, 1f,
                    2, new[] { "cmd_base2" }, new Vector2(-100, -300)),

                // ── Guerrero ──
                N("war_power", "Fuerza bruta", "+15% daño melee.",
                    UpgradeBranch.Guerrero, 0, UpgradeEffectType.IncreaseStatCap, 15f,
                    1, new[] { "cmd_start" }, new Vector2(100, 100)),

                N("war_passive", "Escudo hierro", "Pasiva: reducción daño.",
                    UpgradeBranch.Guerrero, 0, UpgradeEffectType.UnlockPassive, 1f,
                    1, new[] { "war_power" }, new Vector2(50, -100)),

                N("war_order", "Carga heroica", "Orden: embestida en área.",
                    UpgradeBranch.Guerrero, 0, UpgradeEffectType.UnlockOrder, 1f,
                    1, new[] { "war_power" }, new Vector2(200, -100)),

                N("war_statcap", "Veterano", "+20% HP máximo.",
                    UpgradeBranch.Guerrero, 0, UpgradeEffectType.IncreaseStatCap, 20f,
                    2, new[] { "war_passive" }, new Vector2(50, -300)),

                // ── Arquero ──
                N("arc_precision", "Puntería", "+10% precisión.",
                    UpgradeBranch.Arquero, 0, UpgradeEffectType.IncreaseStatCap, 10f,
                    1, new[] { "cmd_start" }, new Vector2(350, 100)),

                N("arc_speed", "Cadencia", "+20% vel. ataque.",
                    UpgradeBranch.Arquero, 0, UpgradeEffectType.IncreaseStatCap, 20f,
                    1, new[] { "arc_precision" }, new Vector2(350, -100)),

                N("arc_order", "Lluvia flechas", "Orden: disparo en área.",
                    UpgradeBranch.Arquero, 0, UpgradeEffectType.UnlockOrder, 1f,
                    2, new[] { "arc_speed" }, new Vector2(350, -300)),

                // ── Mago ──
                N("mage_power", "Canalización", "+15% daño mágico.",
                    UpgradeBranch.Mago, 0, UpgradeEffectType.IncreaseStatCap, 15f,
                    1, new[] { "cmd_start" }, new Vector2(550, 100)),

                N("mage_passive", "Aura mística", "Pasiva: regen maná.",
                    UpgradeBranch.Mago, 0, UpgradeEffectType.UnlockPassive, 1f,
                    1, new[] { "mage_power" }, new Vector2(550, -100)),

                N("mage_order", "Nova arcana", "Orden: explosión mágica.",
                    UpgradeBranch.Mago, 0, UpgradeEffectType.UnlockOrder, 1f,
                    2, new[] { "mage_passive" }, new Vector2(550, -300)),
            };

            var savedNodes = new UpgradeNode[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                var n = nodes[i];
                var path = $"{DataPath}/{n.id}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<UpgradeNode>(path);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(n, existing);
                    EditorUtility.SetDirty(existing);
                    Object.DestroyImmediate(n, true);
                    savedNodes[i] = existing;
                }
                else
                {
                    AssetDatabase.CreateAsset(n, path);
                    savedNodes[i] = AssetDatabase.LoadAssetAtPath<UpgradeNode>(path);
                }
            }

            var treePath = "Assets/_Project/Data/UpgradeTree.asset";
            var tree = AssetDatabase.LoadAssetAtPath<UpgradeTree>(treePath);
            if (tree == null)
            {
                tree = ScriptableObject.CreateInstance<UpgradeTree>();
                AssetDatabase.CreateAsset(tree, treePath);
            }
            tree.nodes = savedNodes;
            EditorUtility.SetDirty(tree);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AutoBattle] Árbol generado: {nodes.Length} nodos, todos gratis para probar.");
        }

        private static UpgradeNode N(string id, string displayName, string desc,
            UpgradeBranch branch, int cost, UpgradeEffectType effect, float effectValue,
            int reqBase, string[] prereqs, Vector2 treePos)
        {
            var node = ScriptableObject.CreateInstance<UpgradeNode>();
            node.name = id;
            node.id = id;
            node.displayName = displayName;
            node.description = desc;
            node.branch = branch;
            node.cost = cost;
            node.effect = effect;
            node.effectValue = effectValue;
            node.requiredBaseLevel = reqBase;
            node.prerequisiteIds = prereqs;
            node.treePosition = treePos;
            if (effect == UpgradeEffectType.UnlockOrder || effect == UpgradeEffectType.UnlockPassive)
                node.effectTargetId = id;
            return node;
        }
    }
}

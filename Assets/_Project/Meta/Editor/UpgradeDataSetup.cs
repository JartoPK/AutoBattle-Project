using System;
using AutoBattle.Meta.Base;
using AutoBattle.Meta.Recruitment;
using AutoBattle.Meta.Upgrades;
using UnityEditor;
using UnityEngine;

namespace AutoBattle.Meta.Editor
{
    /// <summary>
    /// Crea el árbol de mejoras por defecto (Paso 8B) y un test del flujo de
    /// compra: requisitos, efectos meta y flags de combate.
    /// </summary>
    public static class UpgradeDataSetup
    {
        private const string Data = "Assets/_Project/Data";

        [MenuItem("AutoBattle/Setup/Crear árbol de mejoras")]
        public static void CreateUpgradeTree()
        {
            // Asegura el resto de datos (Fase 1 + Base/economía).
            BaseDataSetup.CreateBaseData();
            EnsureFolder(Data, "Upgrades");

            // --- Rama Comandante ---
            var cmdRoster = Node("cmd_roster", "Barracones ampliados", UpgradeBranch.Comandante,
                cost: 150, reqLevel: 1, prereqs: null,
                UpgradeEffectType.IncreaseRosterCap, value: 2, target: null,
                "+2 al tope de roster.");
            var cmdBase2 = Node("cmd_base2", "Ampliar la base", UpgradeBranch.Comandante,
                cost: 250, reqLevel: 1, prereqs: null,
                UpgradeEffectType.IncreaseBaseLevel, value: 1, target: null,
                "Sube el nivel de la base (más tope, mejor herencia, desbloquea nodos).");
            var cmdThirdSlot = Node("cmd_third_slot", "Tercera ranura de orden", UpgradeBranch.Comandante,
                cost: 300, reqLevel: 2, prereqs: new[] { "cmd_base2" },
                UpgradeEffectType.UnlockThirdOrderSlot, value: 0, target: null,
                "[Combate] Habilita una 3.ª ranura de orden por unidad.");
            Node("cmd_arrows", "Orden global: Lluvia de flechas", UpgradeBranch.Comandante,
                cost: 200, reqLevel: 1, prereqs: null,
                UpgradeEffectType.UnlockGlobalOrder, value: 0, target: "lluvia_flechas",
                "[Combate] Desbloquea la orden global Lluvia de flechas.");
            Node("cmd_inherit", "Legado mejorado", UpgradeBranch.Comandante,
                cost: 200, reqLevel: 1, prereqs: null,
                UpgradeEffectType.InheritanceQualityBonus, value: 0.15f, target: null,
                "+15% a las stats heredadas en la herencia.");
            Node("cmd_recruits", "Reclutas de calidad", UpgradeBranch.Comandante,
                cost: 200, reqLevel: 1, prereqs: null,
                UpgradeEffectType.RecruitQualityBonus, value: 0.1f, target: null,
                "Mejora ligeramente las stats de las nuevas reclutas.");

            // --- Ramas por clase (mayormente flags de combate) ---
            Node("war_passive", "Pasiva: Muro", UpgradeBranch.Guerrero,
                cost: 150, reqLevel: 1, prereqs: null,
                UpgradeEffectType.UnlockPassive, value: 0, target: "muro",
                "[Combate] Añade la pasiva 'Muro' al pool del Guerrero.");
            Node("war_statcap", "Aguante de hierro", UpgradeBranch.Guerrero,
                cost: 200, reqLevel: 1, prereqs: null,
                UpgradeEffectType.IncreaseStatCap, value: 0.1f, target: "hp",
                "[Combate] Sube el tope de HP del Guerrero.");
            Node("arc_order", "Orden: Kitear", UpgradeBranch.Arquero,
                cost: 150, reqLevel: 1, prereqs: null,
                UpgradeEffectType.UnlockOrder, value: 0, target: "kitear",
                "[Combate] Añade la orden 'Kitear' al pool del Arquero.");
            Node("mage_passive", "Pasiva: Canalizar", UpgradeBranch.Mago,
                cost: 150, reqLevel: 1, prereqs: null,
                UpgradeEffectType.UnlockPassive, value: 0, target: "canalizar",
                "[Combate] Añade la pasiva 'Canalizar' al pool del Mago.");

            var tree = CreateOrLoad<UpgradeTree>($"{Data}/UpgradeTree.asset");
            tree.nodes = new[]
            {
                cmdRoster, cmdBase2, cmdThirdSlot,
                Load("cmd_arrows"), Load("cmd_inherit"), Load("cmd_recruits"),
                Load("war_passive"), Load("war_statcap"), Load("arc_order"), Load("mage_passive"),
            };
            EditorUtility.SetDirty(tree);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AutoBattle] Árbol de mejoras creado en {Data}/Upgrades ({tree.nodes.Length} nodos).");
        }

        [MenuItem("AutoBattle/Debug/Probar árbol de mejoras")]
        public static void TestUpgrades()
        {
            var tree = AssetDatabase.LoadAssetAtPath<UpgradeTree>($"{Data}/UpgradeTree.asset");
            var baseConfig = AssetDatabase.LoadAssetAtPath<BaseConfig>($"{Data}/BaseConfig.asset");
            if (tree == null || baseConfig == null)
            {
                Debug.LogError("[AutoBattle] Faltan datos. Ejecuta antes 'AutoBattle/Setup/Crear árbol de mejoras'.");
                return;
            }

            var state = MetaGameState.New(startingCoins: 2000);
            Debug.Log($"[AutoBattle] Inicio — base nivel {state.baseLevel}, tope roster {state.GetRosterCap(baseConfig)}, " +
                      $"herencia {state.GetInheritPercent(baseConfig):P0}, monedas {state.wallet.coins}");

            Buy(state, tree, "cmd_roster");
            Debug.Log($"   → tope roster ahora: {state.GetRosterCap(baseConfig)}");

            // Debe FALLAR: requiere base nivel 2 y el prerequisito cmd_base2.
            Buy(state, tree, "cmd_third_slot");

            Buy(state, tree, "cmd_base2");
            Debug.Log($"   → base nivel {state.baseLevel}, tope roster {state.GetRosterCap(baseConfig)}");

            // Ahora SÍ se cumple el requisito.
            Buy(state, tree, "cmd_third_slot");
            Debug.Log($"   → 3.ª ranura desbloqueada (flag combate): " +
                      $"{UpgradeService.IsFlagUnlocked(state, tree, UpgradeEffectType.UnlockThirdOrderSlot)}");

            Buy(state, tree, "cmd_inherit");
            Debug.Log($"   → herencia ahora: {state.GetInheritPercent(baseConfig):P0}");

            Buy(state, tree, "cmd_arrows");
            Buy(state, tree, "arc_order");
            var globals = UpgradeService.GetUnlockedTargets(state, tree, UpgradeEffectType.UnlockGlobalOrder);
            var orders = UpgradeService.GetUnlockedTargets(state, tree, UpgradeEffectType.UnlockOrder);
            Debug.Log($"   → flags de combate para el otro dev → órdenes globales: [{string.Join(", ", globals)}] · órdenes: [{string.Join(", ", orders)}]");

            // Guardado/carga.
            MetaSaveService.Save(state);
            var loaded = MetaSaveService.Load();
            Debug.Log(loaded != null
                ? $"[AutoBattle] Guardado/carga OK → {loaded.upgrades.Count} nodos, base nivel {loaded.baseLevel}, {loaded.wallet.coins} monedas."
                : "[AutoBattle] ERROR: no se pudo recargar la partida.");
        }

        private static void Buy(MetaGameState state, UpgradeTree tree, string nodeId)
        {
            var node = tree.Get(nodeId);
            if (node == null) { Debug.LogWarning($"   ! Nodo '{nodeId}' no existe."); return; }

            var result = UpgradeService.Purchase(state, node);
            Debug.Log(result.success
                ? $"   ✔ Comprado '{node.displayName}' (−{node.cost}) · monedas: {state.wallet.coins}"
                : $"   ✘ '{node.displayName}' no comprable: {result.reason}");
        }

        // ---- helpers ----

        private static UpgradeNode Node(string id, string name, UpgradeBranch branch, int cost, int reqLevel,
            string[] prereqs, UpgradeEffectType effect, float value, string target, string desc)
        {
            var n = CreateOrLoad<UpgradeNode>($"{Data}/Upgrades/{id}.asset");
            n.id = id;
            n.displayName = name;
            n.branch = branch;
            n.cost = cost;
            n.requiredBaseLevel = reqLevel;
            n.prerequisiteIds = prereqs ?? Array.Empty<string>();
            n.effect = effect;
            n.effectValue = value;
            n.effectTargetId = target;
            n.description = desc;
            EditorUtility.SetDirty(n);
            return n;
        }

        private static UpgradeNode Load(string id) =>
            AssetDatabase.LoadAssetAtPath<UpgradeNode>($"{Data}/Upgrades/{id}.asset");

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }
    }
}

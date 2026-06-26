using AutoBattle.Core.Units;
using AutoBattle.Meta.Base;
using AutoBattle.Meta.Recruitment;
using AutoBattle.Meta.Upgrades;
using UnityEditor;
using UnityEngine;

namespace AutoBattle.Meta.Editor
{
    /// <summary>
    /// Crea TODO el contenido por defecto y un <see cref="GameConfig"/> en
    /// Resources, que es lo que la capa App carga en runtime para la UI.
    /// </summary>
    public static class AppDataSetup
    {
        private const string Data = "Assets/_Project/Data";
        private const string Resources = "Assets/_Project/Resources";

        [MenuItem("AutoBattle/Setup/Crear TODO + GameConfig (para la UI)")]
        public static void CreateAll()
        {
            // Cascada: Fase 1 + Base/economía + árbol de mejoras.
            UpgradeDataSetup.CreateUpgradeTree();

            if (!AssetDatabase.IsValidFolder(Resources))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");

            var cfg = CreateOrLoad<GameConfig>($"{Resources}/GameConfig.asset");
            cfg.generationConfig = Load<UnitGenerationConfig>("UnitGenerationConfig");
            cfg.classDatabase = Load<ClassDatabase>("ClassDatabase");
            cfg.rarityConfig = Load<RarityConfig>("RarityConfig");
            cfg.baseConfig = Load<BaseConfig>("BaseConfig");
            cfg.recruitmentConfig = Load<RecruitmentConfig>("RecruitmentConfig");
            cfg.upgradeTree = Load<UpgradeTree>("UpgradeTree");
            cfg.startingCoins = 1000;
            EditorUtility.SetDirty(cfg);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AutoBattle] GameConfig creado en Resources. Pulsa Play para ver el mapa y la base.");
        }

        [MenuItem("AutoBattle/Debug/Borrar partida guardada")]
        public static void DeleteSave()
        {
            MetaSaveService.DeleteSave();
            Debug.Log("[AutoBattle] Partida guardada borrada. La próxima vez que entres en Play empezarás de cero.");
        }

        private static T Load<T>(string name) where T : Object =>
            AssetDatabase.LoadAssetAtPath<T>($"{Data}/{name}.asset");

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

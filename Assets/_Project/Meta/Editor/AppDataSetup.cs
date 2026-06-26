using AutoBattle.Core.Units;
using AutoBattle.Meta.Base;
using AutoBattle.Meta.Campaign;
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
            cfg.campaignMap = CreateCampaign();
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

        // Mapa de campaña de prueba: varios nodos con tipo, dificultad, recompensa,
        // posición en el mapa y una formación enemiga.
        private static CampaignMap CreateCampaign()
        {
            if (!AssetDatabase.IsValidFolder($"{Data}/Campaign"))
                AssetDatabase.CreateFolder(Data, "Campaign");

            var nodes = new[]
            {
                Node("n1", "Vado del Sur", NodeType.Combate, 1, 120, new Vector2(-16f, -9f),
                    Form(UnitClass.Guerrero, UnitClass.Guerrero, UnitClass.Arquero)),
                Node("n2", "Bosque Viejo", NodeType.Combate, 2, 160, new Vector2(-7f, -4f),
                    Form(UnitClass.Arquero, UnitClass.Arquero, UnitClass.Guerrero, UnitClass.Mago)),
                Node("n3", "Aldea de reclutas", NodeType.Reclutamiento, 1, 0, new Vector2(2f, 1f),
                    Form(UnitClass.Guerrero)),
                Node("n4", "Mina abandonada", NodeType.Recursos, 2, 260, new Vector2(11f, 5f),
                    Form(UnitClass.Guerrero, UnitClass.Arquero)),
                Node("n5", "Guarida élite", NodeType.Elite, 3, 320, new Vector2(4f, 11f),
                    Form(UnitClass.Mago, UnitClass.Mago, UnitClass.Arquero, UnitClass.Guerrero, UnitClass.Guerrero)),
                Node("n6", "Paso de la montaña", NodeType.Combate, 3, 240, new Vector2(-11f, 9f),
                    Form(UnitClass.Guerrero, UnitClass.Arquero, UnitClass.Arquero, UnitClass.Mago)),
                Node("n7", "Fortaleza del Jefe", NodeType.Jefe, 5, 600, new Vector2(17f, 16f),
                    Form(UnitClass.Guerrero, UnitClass.Guerrero, UnitClass.Mago, UnitClass.Mago, UnitClass.Arquero, UnitClass.Arquero)),
            };

            var map = CreateOrLoad<CampaignMap>($"{Data}/Campaign/CampaignMap.asset");
            map.nodes = nodes;
            EditorUtility.SetDirty(map);
            return map;
        }

        private static CampaignNodeData Node(string id, string name, NodeType type, int difficulty,
            int reward, Vector2 pos, EnemyUnit[] formation)
        {
            var n = CreateOrLoad<CampaignNodeData>($"{Data}/Campaign/{id}.asset");
            n.id = id;
            n.displayName = name;
            n.type = type;
            n.difficulty = difficulty;
            n.coinReward = reward;
            n.mapPosition = pos;
            n.formation = formation;
            EditorUtility.SetDirty(n);
            return n;
        }

        // Coloca las clases dadas en una rejilla 4x3 (rellenando por filas).
        private static EnemyUnit[] Form(params UnitClass[] classes)
        {
            var result = new EnemyUnit[classes.Length];
            for (int i = 0; i < classes.Length; i++)
                result[i] = new EnemyUnit { unitClass = classes[i], col = i % 4, row = i / 4 };
            return result;
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

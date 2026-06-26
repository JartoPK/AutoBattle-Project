using System;
using AutoBattle.Core.Editor;
using AutoBattle.Core.Units;
using AutoBattle.Meta.Base;
using AutoBattle.Meta.Inheritance;
using AutoBattle.Meta.Recruitment;
using UnityEditor;
using UnityEngine;

namespace AutoBattle.Meta.Editor
{
    /// <summary>
    /// Crea los datos de Base/economía (Paso 8A) y un test del ciclo completo.
    /// Menú "AutoBattle" en la barra superior de Unity.
    /// </summary>
    public static class BaseDataSetup
    {
        private const string Data = "Assets/_Project/Data";

        [MenuItem("AutoBattle/Setup/Crear datos de Base y economía")]
        public static void CreateBaseData()
        {
            // Asegura que existan los datos de la Fase 1 (config + clases + db).
            DefaultDataSetup.CreateDefaults();

            var generationConfig = AssetDatabase.LoadAssetAtPath<UnitGenerationConfig>($"{Data}/UnitGenerationConfig.asset");
            var classDatabase = AssetDatabase.LoadAssetAtPath<ClassDatabase>($"{Data}/ClassDatabase.asset");

            var rarityConfig = CreateOrLoad<RarityConfig>($"{Data}/RarityConfig.asset");
            var baseConfig = CreateOrLoad<BaseConfig>($"{Data}/BaseConfig.asset");

            var recruitment = CreateOrLoad<RecruitmentConfig>($"{Data}/RecruitmentConfig.asset");
            recruitment.generationConfig = generationConfig;
            recruitment.classDatabase = classDatabase;
            recruitment.rarityConfig = rarityConfig;
            recruitment.tiers = DefaultTiers();
            EditorUtility.SetDirty(recruitment);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AutoBattle] Datos de Base y economía creados/actualizados en {Data}");
        }

        [MenuItem("AutoBattle/Debug/Probar Base (reclutar + herencia + guardado)")]
        public static void TestBaseCycle()
        {
            var recruitment = AssetDatabase.LoadAssetAtPath<RecruitmentConfig>($"{Data}/RecruitmentConfig.asset");
            var baseConfig = AssetDatabase.LoadAssetAtPath<BaseConfig>($"{Data}/BaseConfig.asset");
            if (recruitment == null || baseConfig == null)
            {
                Debug.LogError("[AutoBattle] Faltan datos. Ejecuta antes 'AutoBattle/Setup/Crear datos de Base y economía'.");
                return;
            }

            var rng = new System.Random();
            var state = MetaGameState.New(startingCoins: 1000);
            Debug.Log($"[AutoBattle] Nueva partida — monedas: {state.wallet.coins}, tope roster: {baseConfig.GetRosterCap(state.baseLevel)}");

            // Recluta una unidad de cada tier.
            foreach (var tier in recruitment.tiers)
            {
                var result = RecruitmentService.Recruit(state, tier, recruitment, baseConfig, rng);
                if (result.success)
                {
                    var u = result.unit;
                    Debug.Log($"  + Tier '{tier.displayName}' ({tier.cost} mon.) → [{u.rarity}] {u.classId} {u.displayName} · " +
                              $"HP {u.baseStats.hp:F0}, ATQ {u.baseStats.attack:F1}, pasiva '{u.passiveId}' · monedas restantes: {state.wallet.coins}");
                }
                else
                {
                    Debug.LogWarning($"  ! Tier '{tier.displayName}' falló: {result.reason}");
                }
            }

            // Herencia: convierte la 1ª unidad en veterana y la "renace" como Mago.
            if (state.roster.Count > 0)
            {
                var veteran = state.roster.units[0];
                veteran.battlesSurvived = 1;
                var heir = InheritanceService.Inherit(state, veteran.id, UnitClass.Mago, recruitment, baseConfig, rng);
                if (heir != null)
                    Debug.Log($"  ⮡ Herencia de {veteran.displayName} ({veteran.classId}) → {heir.displayName} [{heir.rarity}] {heir.classId}, " +
                              $"hereda pasiva '{heir.passiveId}', HP {heir.baseStats.hp:F0} (≈{baseConfig.statInheritPercent:P0} del veterano)");
            }

            // Guardado/carga: verifica el roundtrip.
            MetaSaveService.Save(state);
            var loaded = MetaSaveService.Load();
            Debug.Log(loaded != null
                ? $"[AutoBattle] Guardado/carga OK → roster {loaded.roster.Count} unidades, {loaded.wallet.coins} monedas."
                : "[AutoBattle] ERROR: no se pudo recargar la partida.");
        }

        // ---- datos por defecto ----

        // Tiers según el diseño: cada nivel desbloquea rarezas mejores.
        private static RecruitmentConfig.RecruitTier[] DefaultTiers() => new[]
        {
            new RecruitmentConfig.RecruitTier
            {
                displayName = "Gratis",
                cost = 0,
                rarityTable = new[]
                {
                    W(Rarity.Comun, 70f),
                    W(Rarity.PocoComun, 30f),
                },
            },
            new RecruitmentConfig.RecruitTier
            {
                displayName = "Estándar",
                cost = 100,
                rarityTable = new[]
                {
                    W(Rarity.Comun, 45f),
                    W(Rarity.PocoComun, 35f),
                    W(Rarity.Epica, 20f),
                },
            },
            new RecruitmentConfig.RecruitTier
            {
                displayName = "Élite",
                cost = 300,
                rarityTable = new[]
                {
                    W(Rarity.Comun, 15f),
                    W(Rarity.PocoComun, 25f),
                    W(Rarity.Rara, 30f),
                    W(Rarity.Epica, 22f),
                    W(Rarity.Legendaria, 8f),
                },
            },
        };

        private static RecruitmentConfig.RarityWeight W(Rarity r, float w) =>
            new() { rarity = r, weight = w };

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

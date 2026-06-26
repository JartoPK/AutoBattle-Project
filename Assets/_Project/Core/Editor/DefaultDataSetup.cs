using System;
using AutoBattle.Core.Units;
using UnityEditor;
using UnityEngine;

namespace AutoBattle.Core.Editor
{
    /// <summary>
    /// Utilidades de editor para arrancar la Fase 1 sin crear assets a mano.
    /// Menú "AutoBattle" en la barra superior de Unity.
    /// </summary>
    public static class DefaultDataSetup
    {
        private const string Root = "Assets/_Project/Data";

        [MenuItem("AutoBattle/Setup/Crear datos por defecto")]
        public static void CreateDefaults()
        {
            EnsureFolder("Assets/_Project", "Data");
            EnsureFolder(Root, "Passives");
            EnsureFolder(Root, "Classes");

            // Baremo universal.
            CreateOrLoad<UnitGenerationConfig>($"{Root}/UnitGenerationConfig.asset");

            // Una pasiva de ejemplo por clase.
            var provocar = CreatePassive("provocar", "Provocar",
                "Los enemigos cercanos tienden a atacar a esta unidad.");
            var certero = CreatePassive("ojo_certero", "Ojo certero",
                "Probabilidad de golpe crítico a distancia.");
            var arcano = CreatePassive("flujo_arcano", "Flujo arcano",
                "Regenera maná más rápido.");

            // Clases iniciales.
            var guerrero = CreateClass(UnitClass.Guerrero, "Guerrero", usesMana: false, new[] { provocar });
            var arquero = CreateClass(UnitClass.Arquero, "Arquero", usesMana: false, new[] { certero });
            var mago = CreateClass(UnitClass.Mago, "Mago", usesMana: true, new[] { arcano });

            // Registro de clases.
            var db = CreateOrLoad<ClassDatabase>($"{Root}/ClassDatabase.asset");
            db.classes = new[] { guerrero, arquero, mago };
            EditorUtility.SetDirty(db);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AutoBattle] Datos por defecto creados/actualizados en {Root}");
        }

        [MenuItem("AutoBattle/Debug/Generar 3 unidades de prueba (log)")]
        public static void GenerateTestUnits()
        {
            var config = AssetDatabase.LoadAssetAtPath<UnitGenerationConfig>($"{Root}/UnitGenerationConfig.asset");
            var db = AssetDatabase.LoadAssetAtPath<ClassDatabase>($"{Root}/ClassDatabase.asset");
            if (config == null || db == null)
            {
                Debug.LogError("[AutoBattle] Faltan datos. Ejecuta antes 'AutoBattle/Setup/Crear datos por defecto'.");
                return;
            }

            var rng = new System.Random();
            foreach (UnitClass cls in Enum.GetValues(typeof(UnitClass)))
            {
                var classData = db.Get(cls);
                if (classData == null) continue;

                var u = UnitFactory.Create(classData, config, rng);
                Debug.Log(
                    $"[{u.classId}] {u.displayName} — HP {u.baseStats.hp:F0}, " +
                    $"ATQ {u.baseStats.attack:F1}, AS {u.baseStats.attackSpeed:F2}/s, " +
                    $"MS {u.baseStats.moveSpeed:F2}, Maná {u.baseStats.mana:F0} · pasiva '{u.passiveId}'");
            }
        }

        // ---- helpers ----

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

        private static PassiveData CreatePassive(string id, string name, string desc)
        {
            var p = CreateOrLoad<PassiveData>($"{Root}/Passives/{id}.asset");
            p.id = id;
            p.displayName = name;
            p.description = desc;
            EditorUtility.SetDirty(p);
            return p;
        }

        private static ClassData CreateClass(UnitClass id, string name, bool usesMana, PassiveData[] passives)
        {
            var c = CreateOrLoad<ClassData>($"{Root}/Classes/{name}.asset");
            c.classId = id;
            c.displayName = name;
            c.usesMana = usesMana;
            c.possiblePassives = passives;
            c.availableOrders ??= Array.Empty<OrderData>();
            c.spells ??= Array.Empty<SpellData>();
            EditorUtility.SetDirty(c);
            return c;
        }
    }
}

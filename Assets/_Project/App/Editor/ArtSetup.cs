using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AutoBattle.App.Editor
{
    public static class ArtSetup
    {
        private const string Resources = "Assets/_Project/Resources";
        private const string TS = "Assets/Tiny Swords";

        [MenuItem("AutoBattle/Setup/Wire arte 2D (Tiny Swords)")]
        public static void WireArt()
        {
            if (!AssetDatabase.IsValidFolder(Resources))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");

            var cfg = CreateOrLoad<ArtConfig>($"{Resources}/ArtConfig.asset");

            cfg.warrior = Anim($"{TS}/Units/Blue Units/Warrior/Warrior_Idle.png", $"{TS}/Units/Blue Units/Warrior/Warrior_Run.png");
            cfg.archer  = Anim($"{TS}/Units/Blue Units/Archer/Archer_Idle.png",   $"{TS}/Units/Blue Units/Archer/Archer_Run.png");
            cfg.monk    = Anim($"{TS}/Units/Blue Units/Monk/Idle.png",            $"{TS}/Units/Blue Units/Monk/Run.png");
            cfg.pawn    = Anim($"{TS}/Pawn and Resources/Pawn/Blue Pawn/Pawn_Idle.png", $"{TS}/Pawn and Resources/Pawn/Blue Pawn/Pawn_Run.png");

            cfg.barracks  = Spr($"{TS}/Buildings/Blue Buildings/Barracks.png");
            cfg.castle    = Spr($"{TS}/Buildings/Blue Buildings/Castle.png");
            cfg.monastery = Spr($"{TS}/Buildings/Blue Buildings/Monastery.png");
            cfg.house     = Spr($"{TS}/Buildings/Blue Buildings/House1.png");

            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AutoBattle] ArtConfig enlazado. Warrior={Count(cfg.warrior)} Archer={Count(cfg.archer)} " +
                      $"Monk={Count(cfg.monk)} Pawn={Count(cfg.pawn)} frames. " +
                      "Para la isla: ejecuta 'AutoBattle/Setup/Crear isla base (Tilemap)'.");
        }

        private static int Count(ArtConfig.UnitAnim a) => a != null && a.idle != null ? a.idle.Length : 0;

        private static ArtConfig.UnitAnim Anim(string idlePath, string runPath) => new()
        {
            idle = Frames(idlePath),
            run = Frames(runPath),
        };

        private static Sprite[] Frames(string path)
        {
            var list = new List<Sprite>();
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                if (o is Sprite s) list.Add(s);
            list.Sort((a, b) => FrameIndex(a.name).CompareTo(FrameIndex(b.name)));
            return list.ToArray();
        }

        private static int FrameIndex(string name)
        {
            int i = name.LastIndexOf('_');
            return i >= 0 && int.TryParse(name.Substring(i + 1), out var n) ? n : 0;
        }

        private static Sprite Spr(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                if (o is Sprite sp) return sp;
            return null;
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

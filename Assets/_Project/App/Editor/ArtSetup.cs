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

            var cursorPath = $"{TS}/UI Elements/Cursors/Cursor_02.png";
            EnsureCursorImport(cursorPath);
            cfg.cursor = AssetDatabase.LoadAssetAtPath<Texture2D>(cursorPath);
            cfg.coinIcon = Spr($"{TS}/UI Elements/Icons/Icon_03.png");
            cfg.coinRibbon = Spr($"{TS}/UI Elements/Ribbons/SmallRibbons 2.png");
            cfg.ribbonButton = Spr($"{TS}/UI Elements/Ribbons/SmallRibbons 4.png");
            cfg.woodTable = Spr($"{TS}/UI Elements/Wood Table/WoodTable_Slots.png");
            cfg.skillNodeButton = Spr($"{TS}/UI Elements/Buttons/TinySquareBlueButton.png");
            cfg.swordsHeader = Spr($"{TS}/UI Elements/Ribbons/SmallRibbons 7.png");
            cfg.detailPaper = Spr($"{TS}/UI Elements/Papers/RegularPaper.png");
            cfg.ribbonVolver = Spr($"{TS}/UI Elements/Ribbons/SmallRibbons 6.png");
            cfg.ribbonBuy = Spr($"{TS}/UI Elements/Ribbons/SmallRibbons 10.png");
            cfg.nodeIconEconomy = Spr($"{TS}/UI Elements/Icons/Icon_03.png");
            cfg.nodeIconWarrior = Spr($"{TS}/UI Elements/Human Avatars/Avatars_01.png");
            cfg.nodeIconArcher = Spr($"{TS}/UI Elements/Human Avatars/Avatars_03.png");
            cfg.nodeIconMage = Spr($"{TS}/UI Elements/Human Avatars/Avatars_04.png");
            cfg.nodeIconRecruits = Spr($"{TS}/UI Elements/Icons/Icon_04.png");

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

        private static void EnsureCursorImport(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            if (importer.isReadable && importer.textureType == TextureImporterType.Cursor) return;
            importer.textureType = TextureImporterType.Cursor;
            importer.isReadable = true;
            importer.SaveAndReimport();
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

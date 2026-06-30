using System;
using AutoBattle.Core.Units;
using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>
    /// Referencias al arte (Tiny Swords) que la capa App usa en runtime. Se rellena
    /// con el menu "AutoBattle/Setup/Wire arte 2D" y se carga desde Resources.
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Art Config", fileName = "ArtConfig")]
    public class ArtConfig : ScriptableObject
    {
        [Serializable]
        public class UnitAnim
        {
            public Sprite[] idle;
            public Sprite[] run;
        }

        [Header("Animaciones de unidad (frames; color azul = jugador)")]
        public UnitAnim warrior;
        public UnitAnim archer;
        public UnitAnim monk;
        public UnitAnim pawn;

        [Header("Color del agua de fondo")]
        public Color waterColor = new(0.31f, 0.56f, 0.62f);

        [Header("Edificios de la base")]
        public Sprite barracks;
        public Sprite castle;
        public Sprite monastery;
        public Sprite house;

        [Header("UI")]
        public Texture2D cursor;
        public Sprite coinIcon;
        public Sprite coinRibbon;
        public Sprite ribbonButton;
        public Sprite woodTable;
        public Sprite skillNodeButton;
        public Sprite swordsHeader;
        public Sprite detailPaper;

        public UnitAnim AnimFor(UnitClass cls) => cls switch
        {
            UnitClass.Guerrero => warrior,
            UnitClass.Arquero => archer,
            UnitClass.Mago => monk,
            _ => null,
        };
    }
}

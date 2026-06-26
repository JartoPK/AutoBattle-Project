using AutoBattle.Core.Units;
using UnityEngine;

namespace AutoBattle.App
{
    /// <summary>Color y nombre legible por rareza (para UI, ruleta y cubos de tropa).</summary>
    public static class RarityVisuals
    {
        public static Color Of(Rarity rarity) => rarity switch
        {
            Rarity.Comun => new Color(0.80f, 0.80f, 0.80f),
            Rarity.PocoComun => new Color(0.40f, 0.85f, 0.40f),
            Rarity.Rara => new Color(0.40f, 0.60f, 1.00f),
            Rarity.Epica => new Color(0.75f, 0.45f, 0.95f),
            Rarity.Legendaria => new Color(1.00f, 0.78f, 0.25f),
            _ => Color.white,
        };

        public static string Name(Rarity rarity) => rarity switch
        {
            Rarity.Comun => "Común",
            Rarity.PocoComun => "Poco común",
            Rarity.Rara => "Rara",
            Rarity.Epica => "Épica",
            Rarity.Legendaria => "Legendaria",
            _ => rarity.ToString(),
        };
    }
}

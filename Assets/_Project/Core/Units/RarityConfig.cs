using System;
using UnityEngine;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Define cómo influye la rareza en las stats: a cada rareza le corresponde
    /// una ventana de percentil [0..1] dentro del baremo universal. Una unidad
    /// común tira en la parte baja del baremo; una legendaria en la alta. Así la
    /// rareza mejora las stats sin salirse del baremo universal ("ADN puro").
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Rarity Config", fileName = "RarityConfig")]
    public class RarityConfig : ScriptableObject
    {
        [Serializable]
        public struct Band
        {
            public Rarity rarity;
            [Tooltip("Ventana de percentil [0..1] del baremo donde se tira cada stat.")]
            public StatRange percentile;
        }

        public Band[] bands = DefaultBands();

        /// <summary>Ventana de percentil para una rareza. [0,1] completo si no está definida.</summary>
        public StatRange GetPercentileWindow(Rarity rarity)
        {
            if (bands != null)
                foreach (var b in bands)
                    if (b.rarity == rarity) return b.percentile;
            return new StatRange(0f, 1f);
        }

        private static Band[] DefaultBands() => new[]
        {
            new Band { rarity = Rarity.Comun,      percentile = new StatRange(0.00f, 0.30f) },
            new Band { rarity = Rarity.PocoComun,  percentile = new StatRange(0.25f, 0.50f) },
            new Band { rarity = Rarity.Rara,       percentile = new StatRange(0.45f, 0.70f) },
            new Band { rarity = Rarity.Epica,      percentile = new StatRange(0.65f, 0.88f) },
            new Band { rarity = Rarity.Legendaria, percentile = new StatRange(0.85f, 1.00f) },
        };
    }
}

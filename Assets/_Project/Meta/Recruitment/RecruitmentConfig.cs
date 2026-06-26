using System;
using AutoBattle.Core.Units;
using UnityEngine;

namespace AutoBattle.Meta.Recruitment
{
    /// <summary>
    /// Configuración del edificio de reclutamiento: los tiers (Gratis / coste X /
    /// coste mayor) y la "ruleta" de rarezas de cada uno. Referencia los datos de
    /// la Fase 1 que necesita para generar unidades.
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Recruitment Config", fileName = "RecruitmentConfig")]
    public class RecruitmentConfig : ScriptableObject
    {
        /// <summary>Peso de una rareza en la ruleta de un tier.</summary>
        [Serializable]
        public struct RarityWeight
        {
            public Rarity rarity;
            public float weight;
        }

        /// <summary>Un nivel de reclutamiento: coste fijo y ruleta de rarezas posibles.</summary>
        [Serializable]
        public class RecruitTier
        {
            public string displayName;
            public int cost;

            [Tooltip("Ruleta: rarezas que pueden salir y su peso relativo.")]
            public RarityWeight[] rarityTable;
        }

        [Header("Dependencias (datos de la Fase 1)")]
        public UnitGenerationConfig generationConfig;
        public ClassDatabase classDatabase;
        public RarityConfig rarityConfig;

        [Header("Tiers de reclutamiento")]
        public RecruitTier[] tiers;
    }
}

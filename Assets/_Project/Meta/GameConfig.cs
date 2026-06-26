using AutoBattle.Core.Units;
using AutoBattle.Meta.Base;
using AutoBattle.Meta.Campaign;
using AutoBattle.Meta.Recruitment;
using AutoBattle.Meta.Upgrades;
using UnityEngine;

namespace AutoBattle.Meta
{
    /// <summary>
    /// Punto único de acceso a todos los datos de configuración en runtime.
    /// Se coloca en una carpeta Resources para cargarlo con
    /// <c>Resources.Load&lt;GameConfig&gt;("GameConfig")</c> sin depender del editor.
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public UnitGenerationConfig generationConfig;
        public ClassDatabase classDatabase;
        public RarityConfig rarityConfig;
        public BaseConfig baseConfig;
        public RecruitmentConfig recruitmentConfig;
        public UpgradeTree upgradeTree;
        public CampaignMap campaignMap;

        [Header("Partida nueva")]
        public int startingCoins = 1000;
    }
}

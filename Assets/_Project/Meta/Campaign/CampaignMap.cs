using UnityEngine;

namespace AutoBattle.Meta.Campaign
{
    /// <summary>El mapa de conquista: todos sus nodos.</summary>
    [CreateAssetMenu(menuName = "AutoBattle/Campaign Map", fileName = "CampaignMap")]
    public class CampaignMap : ScriptableObject
    {
        public CampaignNodeData[] nodes;
    }
}

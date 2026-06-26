using System;
using AutoBattle.Core.Units;
using UnityEngine;

namespace AutoBattle.Meta.Campaign
{
    /// <summary>Tipo de territorio/nodo del mapa de conquista (GDD 4.8).</summary>
    public enum NodeType { Combate, Elite, Jefe, Reclutamiento, Recursos }

    /// <summary>Un enemigo colocado en la rejilla de formación (col, row).</summary>
    [Serializable]
    public struct EnemyUnit
    {
        public UnitClass unitClass;
        public int col;
        public int row;
    }

    /// <summary>
    /// Definición de un nodo del mapa: tipo, dificultad, recompensa, posición en el
    /// mapa y la formación enemiga (de momento solo para mostrarla, sin combate).
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Campaign Node", fileName = "Node")]
    public class CampaignNodeData : ScriptableObject
    {
        public string id;
        public string displayName;
        public NodeType type;

        [Range(1, 5)] public int difficulty = 1;
        public int coinReward;

        [Tooltip("Posición (x, z) del nodo sobre el mapa 3D.")]
        public Vector2 mapPosition;

        [Tooltip("Enemigos y su colocación en una rejilla (col 0-3, row 0-2).")]
        public EnemyUnit[] formation;
    }
}

using System;
using System.Collections.Generic;

namespace AutoBattle.Meta.Upgrades
{
    /// <summary>
    /// Qué nodos ha desbloqueado el jugador. Fuente de verdad de la progresión
    /// del árbol; se guarda dentro del <see cref="MetaGameState"/>.
    /// </summary>
    [Serializable]
    public class UpgradeState
    {
        public List<string> unlockedNodeIds = new();

        public int Count => unlockedNodeIds.Count;

        public bool IsUnlocked(string nodeId) => unlockedNodeIds.Contains(nodeId);

        public void Unlock(string nodeId)
        {
            if (!string.IsNullOrEmpty(nodeId) && !IsUnlocked(nodeId))
                unlockedNodeIds.Add(nodeId);
        }
    }
}

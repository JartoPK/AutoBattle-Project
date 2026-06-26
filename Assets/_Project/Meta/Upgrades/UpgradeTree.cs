using System.Collections.Generic;
using UnityEngine;

namespace AutoBattle.Meta.Upgrades
{
    /// <summary>Conjunto de todos los nodos del árbol. Resuelve nodos por id.</summary>
    [CreateAssetMenu(menuName = "AutoBattle/Upgrade Tree", fileName = "UpgradeTree")]
    public class UpgradeTree : ScriptableObject
    {
        public UpgradeNode[] nodes;

        private Dictionary<string, UpgradeNode> _map;

        public UpgradeNode Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_map == null) Build();
            return _map.TryGetValue(id, out var node) ? node : null;
        }

        public IEnumerable<UpgradeNode> GetByBranch(UpgradeBranch branch)
        {
            if (nodes == null) yield break;
            foreach (var n in nodes)
                if (n != null && n.branch == branch) yield return n;
        }

        private void Build()
        {
            _map = new Dictionary<string, UpgradeNode>();
            if (nodes == null) return;
            foreach (var n in nodes)
                if (n != null && !string.IsNullOrEmpty(n.id)) _map[n.id] = n;
        }

        private void OnEnable() => _map = null;
    }
}

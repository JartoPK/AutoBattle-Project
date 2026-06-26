using System.Collections.Generic;
using UnityEngine;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Registro de todas las clases. Permite resolver una <see cref="UnitClass"/>
    /// (lo que se guarda en cada UnitInstance) a su <see cref="ClassData"/> al
    /// cargar una partida. Asignar las clases en el inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "AutoBattle/Class Database", fileName = "ClassDatabase")]
    public class ClassDatabase : ScriptableObject
    {
        public ClassData[] classes;

        private Dictionary<UnitClass, ClassData> _map;

        public ClassData Get(UnitClass id)
        {
            if (_map == null) Build();
            return _map.TryGetValue(id, out var data) ? data : null;
        }

        private void Build()
        {
            _map = new Dictionary<UnitClass, ClassData>();
            if (classes == null) return;
            foreach (var c in classes)
                if (c != null) _map[c.classId] = c;
        }

        // Fuerza reconstrucción del mapa si se edita el array en el editor.
        private void OnEnable() => _map = null;
    }
}

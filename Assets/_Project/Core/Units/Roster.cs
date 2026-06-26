using System;
using System.Collections.Generic;

namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Colección persistente de unidades del jugador. Contenedor serializable
    /// (lo guarda el SaveSystem). La gestión de alto nivel (reclutamiento, tope
    /// de almacenamiento, herencia) se construirá sobre esto en la Fase 8.
    /// </summary>
    [Serializable]
    public class Roster
    {
        public List<UnitInstance> units = new();

        public int Count => units.Count;

        public void Add(UnitInstance unit)
        {
            if (unit != null) units.Add(unit);
        }

        public bool Remove(string unitId) => units.RemoveAll(u => u.id == unitId) > 0;

        public UnitInstance Get(string unitId) => units.Find(u => u.id == unitId);
    }
}

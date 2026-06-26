using System;
using AutoBattle.Core.Units;
using AutoBattle.Meta.Economy;

namespace AutoBattle.Meta
{
    /// <summary>
    /// Estado dinámico persistente de la partida (capa meta). Es el objeto que el
    /// SaveSystem guarda/carga. La progresión de campaña (Fase 7) y el árbol de
    /// mejoras (Paso 8B) se añadirán aquí como campos adicionales.
    /// </summary>
    [Serializable]
    public class MetaGameState
    {
        public Roster roster = new();
        public Wallet wallet = new();
        public int baseLevel = 1;

        public static MetaGameState New(int startingCoins = 0) => new()
        {
            roster = new Roster(),
            wallet = new Wallet(startingCoins),
            baseLevel = 1,
        };
    }
}

using System;

namespace AutoBattle.Meta.Economy
{
    /// <summary>
    /// Monedero del jugador. Recurso único del MVP ("monedas"). Serializable para
    /// que el SaveSystem lo persista dentro de <see cref="MetaGameState"/>.
    /// </summary>
    [Serializable]
    public class Wallet
    {
        public int coins;

        public Wallet() { }
        public Wallet(int initial) { coins = initial; }

        public bool CanAfford(int amount) => coins >= amount;

        public void Add(int amount)
        {
            if (amount > 0) coins += amount;
        }

        /// <summary>Gasta si hay saldo. Devuelve false (sin tocar nada) si no llega.</summary>
        public bool Spend(int amount)
        {
            if (amount < 0 || coins < amount) return false;
            coins -= amount;
            return true;
        }
    }
}

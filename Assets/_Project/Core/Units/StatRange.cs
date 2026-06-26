using System;

namespace AutoBattle.Core.Units
{
    /// <summary>Rango [min, max] del que se tira una stat al crear la unidad.</summary>
    [Serializable]
    public struct StatRange
    {
        public float min;
        public float max;

        public StatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>Devuelve un valor aleatorio en [min, max]. Acepta un RNG externo para reproducibilidad (seeds de campaña).</summary>
        public float Roll(Random rng)
        {
            if (max <= min) return min;
            return min + (float)rng.NextDouble() * (max - min);
        }
    }
}

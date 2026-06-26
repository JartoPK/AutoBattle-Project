using System;

namespace AutoBattle.Core.Units
{
    /// <summary>Genera nombres para las reclutas. Provisional; ampliar libremente.</summary>
    public static class NameGenerator
    {
        private static readonly string[] Names =
        {
            "Bram", "Edda", "Tobre", "Yara", "Korin", "Mela", "Drust", "Selka",
            "Hagen", "Oria", "Veit", "Runa", "Taric", "Esma", "Bolic", "Nadia",
            "Orin", "Sif", "Garm", "Lyra", "Falk", "Brena", "Doran", "Hilde",
        };

        public static string Next(Random rng) => Names[rng.Next(Names.Length)];
    }
}

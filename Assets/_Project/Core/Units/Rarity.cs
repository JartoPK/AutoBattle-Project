namespace AutoBattle.Core.Units
{
    /// <summary>
    /// Calidad de una unidad/pasiva. El orden importa: se usa para comparar
    /// (una unidad solo puede tener pasivas de rareza &lt;= la suya).
    /// </summary>
    public enum Rarity
    {
        Comun,
        PocoComun,
        Rara,
        Epica,
        Legendaria
    }
}

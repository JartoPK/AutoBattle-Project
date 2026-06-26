namespace AutoBattle.Core.Events
{
    /// <summary>
    /// Marcador para todo evento que viaje por el <see cref="EventBus"/>.
    /// Implementarlo como struct (no class) para evitar basura de GC en combate.
    /// </summary>
    public interface IGameEvent { }
}

using AutoBattle.Meta;

namespace AutoBattle.App
{
    /// <summary>
    /// Estado vivo de la sesión para la UI: la configuración (datos) y la partida
    /// actual. Centraliza el guardado para que cualquier pantalla persista cambios.
    /// </summary>
    public class GameContext
    {
        public GameConfig Config;
        public MetaGameState State;

        public void Save() => MetaSaveService.Save(State);
    }
}

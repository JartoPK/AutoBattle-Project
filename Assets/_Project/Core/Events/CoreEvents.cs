namespace AutoBattle.Core.Events
{
    /// <summary>Se dispara cada vez que cambia el estado global del juego.</summary>
    public readonly struct GameStateChangedEvent : IGameEvent
    {
        public readonly GameState Previous;
        public readonly GameState Current;

        public GameStateChangedEvent(GameState previous, GameState current)
        {
            Previous = previous;
            Current = current;
        }
    }
}

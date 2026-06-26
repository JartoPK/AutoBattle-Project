using AutoBattle.Core.Events;
using UnityEngine;

namespace AutoBattle.Core
{
    /// <summary>
    /// Singleton persistente que mantiene el estado global del juego y notifica
    /// los cambios por el <see cref="EventBus"/>. Punto de entrada único de la
    /// capa Core; otros sistemas consultan <see cref="Instance"/> o se suscriben
    /// a <see cref="GameStateChangedEvent"/> en lugar de referenciarse entre sí.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;

        private void Awake()
        {
            // Garantiza una única instancia que sobrevive a los cambios de escena.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Cambia el estado global y publica el evento correspondiente.</summary>
        public void ChangeState(GameState next)
        {
            if (next == State) return;

            var previous = State;
            State = next;
            EventBus.Publish(new GameStateChangedEvent(previous, next));
        }
    }
}

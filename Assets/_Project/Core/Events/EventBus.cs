using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoBattle.Core.Events
{
    /// <summary>
    /// Bus de eventos estático y tipado. Permite que sistemas se comuniquen sin
    /// conocerse entre sí (la capa Meta y la de Combat hablan por aquí sin
    /// referenciarse). Uso:
    ///     EventBus.Subscribe&lt;UnitDiedEvent&gt;(OnUnitDied);
    ///     EventBus.Publish(new UnitDiedEvent(...));
    ///     EventBus.Unsubscribe&lt;UnitDiedEvent&gt;(OnUnitDied);   // p.ej. en OnDisable
    /// Pensado para el hilo principal de Unity (no es thread-safe).
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;
            _handlers.TryGetValue(typeof(T), out var existing);
            _handlers[typeof(T)] = (Action<T>)existing + handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;
            if (!_handlers.TryGetValue(typeof(T), out var existing)) return;

            var updated = (Action<T>)existing - handler;
            if (updated == null) _handlers.Remove(typeof(T));
            else _handlers[typeof(T)] = updated;
        }

        public static void Publish<T>(T evt) where T : IGameEvent
        {
            if (!_handlers.TryGetValue(typeof(T), out var existing)) return;
            if (existing is not Action<T> action) return;

            // Invocamos uno a uno para que una excepción en un suscriptor no
            // impida que los demás reciban el evento.
            foreach (var d in action.GetInvocationList())
            {
                try { ((Action<T>)d).Invoke(evt); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        /// <summary>Borra todas las suscripciones. Útil al reiniciar partida o en tests.</summary>
        public static void Clear() => _handlers.Clear();
    }
}

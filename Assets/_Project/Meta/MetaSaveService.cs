using AutoBattle.Core.Save;

namespace AutoBattle.Meta
{
    /// <summary>
    /// Persiste el <see cref="MetaGameState"/> a través del SaveSystem de Core.
    /// Punto único de guardado/carga de la partida.
    /// </summary>
    public static class MetaSaveService
    {
        private const string Key = "savegame";

        public static void Save(MetaGameState state) => SaveSystem.Save(Key, state);

        public static MetaGameState Load() =>
            SaveSystem.TryLoad<MetaGameState>(Key, out var state) ? state : null;

        public static bool Exists() => SaveSystem.Exists(Key);

        public static void DeleteSave() => SaveSystem.Delete(Key);
    }
}

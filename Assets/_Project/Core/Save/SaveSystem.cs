using System;
using System.IO;
using UnityEngine;

namespace AutoBattle.Core.Save
{
    /// <summary>
    /// Guardado/carga de estado dinámico (roster, progreso de campaña, recursos,
    /// cicatrices) en JSON dentro de <c>Application.persistentDataPath/saves</c>.
    /// Cada bloque se identifica por una clave (un fichero .json por clave).
    ///
    /// MVP con <see cref="JsonUtility"/>: serializa clases [Serializable] con
    /// campos públicos. NO soporta Dictionary ni polimorfismo; si más adelante
    /// hace falta, migrar a Newtonsoft.Json sin tocar las llamadas externas.
    /// </summary>
    public static class SaveSystem
    {
        private static string SaveDir => Path.Combine(Application.persistentDataPath, "saves");
        private static string PathFor(string key) => Path.Combine(SaveDir, key + ".json");

        public static void Save<T>(string key, T data)
        {
            try
            {
                Directory.CreateDirectory(SaveDir);
                File.WriteAllText(PathFor(key), JsonUtility.ToJson(data, prettyPrint: true));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] No se pudo guardar '{key}': {e}");
            }
        }

        public static bool TryLoad<T>(string key, out T data)
        {
            data = default;
            var path = PathFor(key);
            if (!File.Exists(path)) return false;

            try
            {
                data = JsonUtility.FromJson<T>(File.ReadAllText(path));
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] No se pudo cargar '{key}': {e}");
                return false;
            }
        }

        public static bool Exists(string key) => File.Exists(PathFor(key));

        public static void Delete(string key)
        {
            var path = PathFor(key);
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>Borra TODAS las partidas guardadas. Para "nueva partida" o tests.</summary>
        public static void DeleteAll()
        {
            if (Directory.Exists(SaveDir)) Directory.Delete(SaveDir, recursive: true);
        }
    }
}

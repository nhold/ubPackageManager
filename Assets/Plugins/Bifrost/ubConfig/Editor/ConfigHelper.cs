using System.IO;
using UnityEngine;

namespace Bifrost.ubConfig
{
    public static class ConfigHelper
    {
        public static void SaveConfig<T>(string name, T config, string directory = "Config", string extension = ".json")
        {
            File.WriteAllText(directory + name + extension, JsonUtility.ToJson(config));
        }

        public static T LoadConfig<T>(string name, string directory = "Config", string extension = ".json") where T : new()
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            T config = new T();

            if (File.Exists(directory + name + extension))
            {
                config = JsonUtility.FromJson<T>(File.ReadAllText(directory + name + extension));
            }

            return config;
        }
    }
}

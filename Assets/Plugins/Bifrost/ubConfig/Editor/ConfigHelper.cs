using System.IO;
using UnityEngine;

namespace Bifrost.ubConfig
{
    public static class ConfigHelper
    {
        private static string CONFIG_DIRECTORY = "Bifrost/";
        private static string CONFIG_EXTENSION = ".json";

        public static void SaveConfig<T>(string name, T config)
        {
            File.WriteAllText(CONFIG_DIRECTORY+name+CONFIG_EXTENSION, JsonUtility.ToJson(config));
        }

        public static T LoadConfig<T>(string name) where T : new()
        {
            if (!Directory.Exists("Bifrost"))
                Directory.CreateDirectory("Bifrost");

            T config = new T();

            if (File.Exists(CONFIG_DIRECTORY + name + CONFIG_EXTENSION))
            {
                config = JsonUtility.FromJson<T>(File.ReadAllText(CONFIG_DIRECTORY + name + CONFIG_EXTENSION));
            }

            return config;
        }
    }
}

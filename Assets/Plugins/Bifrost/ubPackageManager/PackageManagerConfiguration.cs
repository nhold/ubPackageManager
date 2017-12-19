using System.Collections.Generic;

namespace Bifrost.ubPackageManager
{
    public class PackageManagerConfiguration
    {
        public string lastUpdated = "Never";
        public string pluginDownloadDirectory = "Bifrost/Pacman/Repos";
        public string pluginInstallDirectory = "Assets/Plugins";
        public List<Repository> repositories = new List<Repository>();

        public const float VERSION = 2.0f;
    }
}

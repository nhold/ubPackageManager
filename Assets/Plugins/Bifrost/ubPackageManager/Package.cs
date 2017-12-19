using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Bifrost.ubPackageManager
{
    [Serializable]
    public class PackageVersion
    {
        public string version;
        public string branch;
    }

    [Serializable]
    public class PackageInstallInfo
    {
        public PackageVersion installedVersion;
        public Package package;
    }

    [Serializable]
    public class Package
    {
        public string name;
        public List<PackageVersion> versions;
        public string description;
        public string location;
        public string parentDir;
        public string childDir;

        public List<string> dependencies = new List<string>();

        public void Install(string downloadDirectory, string pluginDirectory)
        {
            string packageInstallDirectory = pluginDirectory + "/" + parentDir + "/" + name;
            string packageDownloadDirectory = downloadDirectory + "/" + parentDir + "/" + name;

            if (Repository.URIChecker.CheckURI(location) == Repository.URIChecker.URIType.DIRECTORY)
            {
                if (Directory.Exists(location))
                {
                    Uninstall(pluginDirectory);
                    Repository.DirectoryCopy(location, packageInstallDirectory);
                }
            }

            if(Repository.URIChecker.CheckURI(location) == Repository.URIChecker.URIType.GIT)
            {
                Debug.Log("Package directory: " + packageInstallDirectory);
                Debug.Log("Temporary directory: " + packageDownloadDirectory);
                Debug.Log("Location: " + location);

                Uninstall(pluginDirectory);

                string stdOut;
                string stdError;
                Repository.GitUpdateRepo(packageDownloadDirectory, location, out stdOut, out stdError);
                Debug.Log(stdOut);
                Debug.LogError(stdError);
 
                Repository.DirectoryCopy(packageDownloadDirectory + "/" + childDir, packageInstallDirectory);
            }
        }

        public void Uninstall(string pluginDirectory)
        {
            string packageInstallDirectory = pluginDirectory + "/" + parentDir + "/" + name;
            if(Directory.Exists(packageInstallDirectory))
            {
                Directory.Delete(packageInstallDirectory, true);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace Bifrost.ubPackageManager
{
    [Serializable]
    public class Package
    {
        public string name;
        public string version;
        public string description;
        public string location;
        public string parentDir;
        public string childDir;

        public List<string> dependencies = new List<string>();

        public void Install(string tempDirectory, string pluginDirectory)
        {
            string packageDirectory = pluginDirectory + "/" + parentDir + "/" + name;
            string temporaryPackageDir = tempDirectory + "/" + parentDir + "/" + name;

            if (Directory.Exists(temporaryPackageDir))
            {
                Directory.Delete(temporaryPackageDir, true);
            }

            Directory.CreateDirectory(temporaryPackageDir);

            if (Repository.URIChecker.CheckURI(location) == Repository.URIChecker.URIType.DIRECTORY)
            {
                if (Directory.Exists(location))
                {
                    Repository.DirectoryCopy(location, temporaryPackageDir);

                    if (Directory.Exists(packageDirectory))
                        Directory.Delete(packageDirectory, true);

                    Repository.DirectoryCopy(temporaryPackageDir, packageDirectory);
                    Directory.Delete(temporaryPackageDir, true);

                    // AssetDatabase.Refresh(); - Add to post install on PM.
                }
            }

            if(Repository.URIChecker.CheckURI(location) == Repository.URIChecker.URIType.GIT)
            {
                // Clone to temp dir

            }
        }
    }
}

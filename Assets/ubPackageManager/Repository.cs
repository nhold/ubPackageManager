using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Bifrost.ubPackageManager
{
    // Should packages in different repos get deps from other repos?
    [Serializable]
    public class Repository
    {
        public class URIChecker
        {
            public enum URIType
            {
                DIRECTORY,
                FILE,
                GIT,
                ERROR
            }

            public static URIType CheckURI(string uri)
            {
                if(uri.Contains("git://") || uri.Contains(".git"))
                {
                    return URIType.GIT;
                }

                if(Directory.Exists(uri))
                {
                    return URIType.DIRECTORY;
                }

                if(File.Exists(uri))
                {
                    return URIType.FILE;
                }

                return URIType.ERROR;
            }
        }

        [SerializeField]
        private string uri;
        public string URI
        {
            get { return uri; }
        }

        [SerializeField]
        private string finalURI;
        public string FinalURI
        {
            get
            {
                return finalURI;
            }
        }

        [SerializeField]
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
        }
        [SerializeField]
        private string lastUpdated;

        private List<Package> packages = new List<Package>();
        public List<Package> Packages
        {
            get { return packages; }
        }

        [SerializeField]
        private List<Package> installedPackages = new List<Package>();
        public List<Package> InstalledPackages
        {
            get
            {
                return installedPackages;
            }
        }

        public Repository(string uri, string name)
        {
            this.uri = uri;
            this.name = name;
        }

        public void ExecuteUpdate(string tempDirectory)
        {
            packages.Clear();
            lastUpdated = DateTime.Now.ToString();

            if (URIChecker.CheckURI(URI) == URIChecker.URIType.DIRECTORY)
            {
                foreach (var str in Directory.GetFiles(URI))
                {
                    Package pack = new Package();
                    JsonUtility.FromJsonOverwrite(File.ReadAllText(str), pack);
                    packages.Add(pack);
                }
            }

            if (URIChecker.CheckURI(URI) == URIChecker.URIType.GIT)
            {
                finalURI = tempDirectory + "/" + name;

                ProcessStartInfo gitInfo = new ProcessStartInfo();
                gitInfo.CreateNoWindow = false;
                gitInfo.RedirectStandardError = true;
                gitInfo.RedirectStandardOutput = true;
                gitInfo.WorkingDirectory = finalURI;
                gitInfo.FileName = "C:\\Program Files (x86)\\Git\\bin\\git.exe";
                gitInfo.UseShellExecute = false;
                Process gitProcess = new Process();

                if (!Directory.Exists(finalURI))
                {
                    Directory.CreateDirectory(finalURI);
                    gitInfo.Arguments = "clone " + uri;
                }
                else
                {
                    gitInfo.Arguments = "pull";
                }

                gitProcess.StartInfo = gitInfo;
                gitProcess.Start();

                string stderr_str = gitProcess.StandardError.ReadToEnd();
                string stdout_str = gitProcess.StandardOutput.ReadToEnd();

                gitProcess.WaitForExit();
                gitProcess.Close();

                UnityEngine.Debug.Log(stdout_str);
                UnityEngine.Debug.LogError(stderr_str);

                foreach (var str in Directory.GetFiles(finalURI + "/packagerepo"))
                {
                    if (str.Contains(".json"))
                    {
                        Package pack = new Package();
                        JsonUtility.FromJsonOverwrite(File.ReadAllText(str), pack);
                        packages.Add(pack);
                    }
                }
            }
        }

        public void InstallPackage(string temp, string pluginDir, string packageName, List<string> alreadyInstalled = null)
        {
            Package packageToInstall = FindPackageByName(packageName);
            if(packageToInstall != null)
            {
                // Install dependencies.
                foreach(var packName in packageToInstall.dependencies)
                {
                    if (alreadyInstalled == null)
                        alreadyInstalled = new List<string>();

                    if (alreadyInstalled.FirstOrDefault(str => str == packName) == null)
                    {
                        InstallPackage(temp, pluginDir, packName, alreadyInstalled);
                    }
                }

                installedPackages.Add(packageToInstall);
                packageToInstall.Install(temp, pluginDir);
            }
        }

        public Package FindPackageByName(string name)
        {
            return packages.FirstOrDefault(pack => pack.name == name);
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}

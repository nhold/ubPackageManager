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
        public string LastUpdated
        {
            get
            {
                return lastUpdated;
            }
             
        }

        [SerializeField]
        private List<Package> packages = new List<Package>();
        public List<Package> Packages
        {
            get { return packages; }
        }

        [SerializeField]
        private List<PackageInstallInfo> installedPackages = new List<PackageInstallInfo>();
        public List<PackageInstallInfo> InstalledPackages
        {
            get
            {
                return installedPackages;
            }

            set
            {
                installedPackages = value;
            }
        }

        public Repository(string uri, string name)
        {
            this.uri = uri;
            this.name = name;
        }

        public void ExecuteUpdate(string tempDirectory)
        {
            if (packages == null)
                packages = new List<Package>();

            if (installedPackages == null)
                installedPackages = new List<PackageInstallInfo>();

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

                string stdOut;
                string stdError;
                GitUpdateRepo(finalURI, uri, out stdOut, out stdError);
                UnityEngine.Debug.Log(stdOut);
                UnityEngine.Debug.LogError(stdError);

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

        public void InstallPackage(string downloadDirectory, string pluginDir, string packageName, int versionID = 0, List<string> alreadyInstalled = null)
        {
            Package packageToInstall = FindPackageByName(packageName);
            if(packageToInstall != null)
            {
                // Install dependencies.
                foreach(var packName in packageToInstall.dependencies)
                {
                    if (alreadyInstalled == null)
                        alreadyInstalled = new List<string>();

                    // TODO: Make sure versions match.
                    if (alreadyInstalled.FirstOrDefault(str => str == packName) == null)
                    {
                        InstallPackage(downloadDirectory, pluginDir, packName, 0, alreadyInstalled);
                    }
                }

                var installed = installedPackages.Find(x => x.package == packageToInstall);
                if(installed != null)
                    installedPackages.Remove(installed);

                installedPackages.Add(new PackageInstallInfo()
                {
                    package = packageToInstall,
                    installedVersion = packageToInstall.versions[versionID]
                });

                packageToInstall.Install(downloadDirectory, pluginDir);
            }
        }

        public void UninstallPackage(string downloadDirectory, string pluginInstallDirectory, string packageName)
        {
            var packageToUninstall = FindPackageByName(packageName);

            if (packageToUninstall != null)
            {
                packageToUninstall.Uninstall(pluginInstallDirectory);
                var installed = installedPackages.Find(x => x.package.name == packageToUninstall.name);
                if (installed != null)
                    installedPackages.Remove(installed);
            }
        }

        public Package FindPackageByName(string name)
        {
            return packages.FirstOrDefault(pack => pack.name == name);
        }

        // HACK: Pull this into a file IO helper.
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
                    // HACK: Please make this an argument.
                    if (subdir.Name == "Plugins")
                        return;

                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static string GetRepoNameFromURL(string url)
        {
            int nameStart = url.LastIndexOf("/");
            int nameEnd = url.LastIndexOf(".git");
            return url.Substring(nameStart, nameEnd - nameStart + 1);
        }

        public static void GitUpdateRepo(string workingDirectory, string uri, out string stdout, out string stderr)
        {
            ProcessStartInfo gitInfo = new ProcessStartInfo();
            gitInfo.WorkingDirectory = workingDirectory;

            string arguments = "clone " + uri;
            
            if (Directory.Exists(workingDirectory))
            {
                string name = GetRepoNameFromURL(uri);
                if (Directory.Exists(workingDirectory + name))
                {
                    gitInfo.WorkingDirectory = workingDirectory + name;
                    arguments = "pull " + uri;
                }
            }
            else
            {
                Directory.CreateDirectory(workingDirectory);
            }

            gitInfo.CreateNoWindow = false;
            gitInfo.RedirectStandardError = true;
            gitInfo.RedirectStandardOutput = true;
            
            gitInfo.FileName = "git";

            // TODO: Figure out ssh credentials.
            //gitInfo.EnvironmentVariables.Add("GIT_SSH_COMMAND", "ssh -i C:\\Users\\Nathan\\.ssh\\id_rsa");

            // TODO: Potentially select a default home path for download?
            // Or use to finde the .ssh key.
            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX)
    ? Environment.GetEnvironmentVariable("HOME")
    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            UnityEngine.Debug.Log(homePath);

            gitInfo.EnvironmentVariables.Add("HOME", homePath);
            gitInfo.UseShellExecute = false;
            gitInfo.Arguments = arguments;

            Process gitProcess = new Process();
            gitProcess.StartInfo = gitInfo;
            gitProcess.Start();

            stderr = gitProcess.StandardError.ReadToEnd();
            stdout = gitProcess.StandardOutput.ReadToEnd();

            gitProcess.WaitForExit();
            gitProcess.Close();
        }

        public static void GitChangeBranch(string workingDirectory, string branch, out string stdout, out string stderr)
        {

            string arguments = "checkout " + branch;

            if (!Directory.Exists(workingDirectory))
            {
                stdout = "Failed to checkout.";
                stderr = "Failed to checkout.";
                return;
            }

            ProcessStartInfo gitInfo = new ProcessStartInfo();
            gitInfo.CreateNoWindow = false;
            gitInfo.RedirectStandardError = true;
            gitInfo.RedirectStandardOutput = true;
            gitInfo.WorkingDirectory = workingDirectory;
            gitInfo.FileName = "C:\\Program Files (x86)\\Git\\bin\\git.exe";
            //gitInfo.EnvironmentVariables.Add("GIT_SSH_COMMAND", "ssh -i C:\\Users\\Nathan\\.ssh\\id_rsa");

            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX)
    ? Environment.GetEnvironmentVariable("HOME")
    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            UnityEngine.Debug.Log(homePath);

            gitInfo.EnvironmentVariables.Add("HOME", homePath);
            gitInfo.UseShellExecute = false;
            gitInfo.Arguments = arguments;

            Process gitProcess = new Process();
            gitProcess.StartInfo = gitInfo;
            gitProcess.Start();

            stderr = gitProcess.StandardError.ReadToEnd();
            stdout = gitProcess.StandardOutput.ReadToEnd();

            gitProcess.WaitForExit();
            gitProcess.Close();
        }
    }
}

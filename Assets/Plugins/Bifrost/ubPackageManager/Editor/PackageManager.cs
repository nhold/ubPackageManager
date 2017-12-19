using UnityEngine;
using UnityEditor;
using Bifrost.ubConfig;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Bifrost.ubPackageManager
{
    public class PackageInstallData
    {
        public Repository repository;
        public Package package;
        public int versionIndex;
    }

    public class PackageManager : EditorWindow
    {
        static PackageManager mWindow;

        public PackageManagerConfiguration PackManConfig
        {
            get;
            set;
        }

        public const string PLUGIN_DIRECTORY = "Assets/Plugins";
        public const string PLUGIN_TEMP_DIRECTORY = "Bifrost/Temp";

        public const string REPO_DIR = "Bifrost/Pacman/Repos/";
        public const string REPO_TEMP_DIR = "Bifrost/Pacman/Repos/";

        private List<Package> packages = new List<Package>();

        private Vector2 scrollPosition = Vector2.zero;

        private Repository repository;

        [MenuItem("Bifrost/Package Manager")]
        public static void GetWindow()
        {
            // Get existing open window or if none, make a new one:
            mWindow = EditorWindow.GetWindow<PackageManager>("Package Manager");
            mWindow.position = new Rect(200, 200, 500, 350);
            mWindow.PackManConfig = ConfigHelper.LoadConfig<PackageManagerConfiguration>("PacmanConfig");
            mWindow.InitialiseRepos();
        }

        private void InitialiseRepos()
        {
            if (Directory.Exists(REPO_DIR))
            {
                foreach (var str in Directory.GetFiles(REPO_DIR))
                {
                    Repository repo = new Repository("", "");
                    JsonUtility.FromJsonOverwrite(File.ReadAllText(str), repo);
                    repository = repo;
                    break;
                }
            }
        }

        private bool manageRepositories = false;
        void OnGUI()
        {
            if (PackManConfig == null)
                PackManConfig = ConfigHelper.LoadConfig<PackageManagerConfiguration>("PacmanConfig");

            StatusBarGUI();

            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Manage Repositories"))
            {
                manageRepositories = true;
            }

            if (GUILayout.Button("Manage Packages"))
            {
                manageRepositories = false;
            }

            GUILayout.EndHorizontal();

            if (manageRepositories)
            {
                RepositoryManageGUI();
            }
            else
            {
                PackageManageGUI();
            }
        }

        private Vector2 scrollPackages = Vector2.zero;
        private void PackageManageGUI()
        {
            if (PackManConfig != null)
            {
                if (PackManConfig.repositories.Count > 0)
                {
                    scrollPackages = EditorGUILayout.BeginScrollView(scrollPackages);
                    foreach (var repo in PackManConfig.repositories)
                    {
                        if (repo.Packages != null && repo.Packages.Count > 0)
                        {

                            foreach (var pack in repo.Packages)
                            {
                                PackageGUI(pack, repo);
                            }
                            
                        }
                        else
                        {
                            GUILayout.BeginVertical("Box");
                            GUI.color = Color.red;

                            if (GUILayout.Button("Update Repo: " + repo.Name))
                            {
                                repo.ExecuteUpdate(PackManConfig.pluginDownloadDirectory);
                                ubConfig.ConfigHelper.SaveConfig("PacmanConfig", PackManConfig);
                            }

                            GUI.color = Color.white;
                            GUILayout.EndVertical();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private void PackageGUI(Package package, Repository repository)
        {
            GUILayout.BeginVertical("Box");

            GUILayout.Label("Name: " + package.name);
            GUILayout.Label("Description: " + package.description);

            if (package.dependencies.Count > 0)
                GUILayout.Label("Dependencies: " + String.Join(", ", package.dependencies.ToArray()));
            else
                GUILayout.Label("No dependencies!");

            if(package.versions.Count > 0)
                GUILayout.Label("Version: " + package.versions[package.versions.Count-1].version);

            if(repository.InstalledPackages == null)
            {
                repository.InstalledPackages = new List<PackageInstallInfo>();
            }

            var installedPackage = repository.InstalledPackages.Where(x => x.package.name == package.name);

            if (installedPackage.Count() > 0)
            {
                if (GUILayout.Button("Uninstall"))
                {
                    
                    repository.UnInstallPackage(PackManConfig.pluginDownloadDirectory, PackManConfig.pluginInstallDirectory, package.name);
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                if (GUILayout.Button("Install"))
                {
                    GenericMenu menu = new GenericMenu();
                    for(int i = 0; i<package.versions.Count; i++)
                    {
                        var version = package.versions[i];

                        PackageInstallData data = new PackageInstallData();
                        data.package = package;
                        data.versionIndex = i;
                        data.repository = repository;

                        menu.AddItem(new GUIContent("Branch: " + version.branch + " Version: " + version.version), false, Install, data);
                    }
                    menu.ShowAsContext();
                    
                }
            }

            GUILayout.EndVertical();
        }

        private void Install(object userData)
        {
            var actualData = userData as PackageInstallData;

            actualData.repository.InstallPackage(PackManConfig.pluginDownloadDirectory, PackManConfig.pluginInstallDirectory, actualData.package.name, actualData.versionIndex);
            AssetDatabase.Refresh();
        }

        private bool addingRepository = false;

        private string newRepoName = "New Name";
        private string newRepoURI = "URI";
        private Repository repoToRemove = null;

        private void RepositoryManageGUI()
        {
            if (PackManConfig != null)
            {
                foreach (var repos in PackManConfig.repositories)
                {
                    GUILayout.BeginVertical("Box");
                    GUILayout.Label("Name: " + repos.Name);
                    GUILayout.Label("URI: " + repos.URI);
                    GUILayout.Label("Last Updated: " + repos.LastUpdated);
                    if (repos.Packages != null)
                        GUILayout.Label("Package Count: " + (repos.Packages.Count));

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Remove"))
                    {
                        repoToRemove = repos;
                    }

                    if (GUILayout.Button("Update"))
                    {
                        repos.ExecuteUpdate(PackManConfig.pluginDownloadDirectory);
                        ubConfig.ConfigHelper.SaveConfig("PacmanConfig", PackManConfig);
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }

                if (repoToRemove != null)
                {
                    PackManConfig.repositories.Remove(repoToRemove);
                    ConfigHelper.SaveConfig("PacmanConfig", PackManConfig);
                    repoToRemove = null;
                }

                if (GUILayout.Button("Add Repo"))
                {
                    addingRepository = true;
                }
                if (addingRepository)
                {
                    newRepoName = EditorGUILayout.TextField("Repo Name: ", newRepoName);
                    newRepoURI = EditorGUILayout.TextField("Repo UI: ", newRepoURI);

                    if (GUILayout.Button("Confirm"))
                    {
                        PackManConfig.repositories.Add(new Repository(newRepoURI, newRepoName));
                        ConfigHelper.SaveConfig("PacmanConfig", PackManConfig);
                        addingRepository = false;
                    }
                }
            }
        }

        private void AddRepo()
        {
            // TODO: Allow customisation of this.
            string uri = EditorUtility.OpenFolderPanel("Repo", "", "");

            if (string.IsNullOrEmpty(uri))
                uri = "https://nhold@bitbucket.org/bifroststudios/packagerepo.git";

            repository = new Repository(uri, "TwoRepo");
            repository.ExecuteUpdate(REPO_TEMP_DIR);

            if (!Directory.Exists(REPO_DIR))
                Directory.CreateDirectory(REPO_DIR);

            File.WriteAllText(REPO_DIR + "/" + repository.Name + ".json", JsonUtility.ToJson(repository));
        }

        private void StatusBarGUI()
        {
            GUILayout.BeginHorizontal("Box");
            GUILayout.BeginVertical();
            GUILayout.Label("ubPackageManager Version: " + PackageManagerConfiguration.VERSION + ". Last Updated: " + PackManConfig.lastUpdated);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Download Directory " + PackManConfig.pluginDownloadDirectory);
            if (GUILayout.Button("Change"))
            {
                string str = EditorUtility.OpenFolderPanel("Choose Download Directory", PackManConfig.pluginDownloadDirectory, "");
                if (!String.IsNullOrEmpty(str))
                {
                    PackManConfig.pluginDownloadDirectory = str;
                    ConfigHelper.SaveConfig("PacmanConfig", PackManConfig);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

        }
    }
}

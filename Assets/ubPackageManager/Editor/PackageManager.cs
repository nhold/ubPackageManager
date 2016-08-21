using UnityEngine;
using UnityEditor;
using Bifrost.ubConfig;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Bifrost.ubPackageManager
{
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
            foreach (var str in Directory.GetFiles(REPO_DIR))
            {
                Repository repo = new Repository("","");
                JsonUtility.FromJsonOverwrite(File.ReadAllText(str), repo);
                repository = repo;
                break;
            }
        }

        void OnGUI()
        {
            if (PackManConfig == null)
                PackManConfig = ConfigHelper.LoadConfig<PackageManagerConfiguration>("PacmanConfig");

            if (repository == null)
            {
                if (GUILayout.Button("Add Repo"))
                {
                    AddRepo();
                }
            }
            else
            {
                GUILayout.Label("Package Manager. Last Updated: " + PackManConfig.lastUpdated);

                GUILayout.BeginHorizontal();
                GUILayout.Label(repository.URI);

                if (GUILayout.Button("Change Repo"))
                {
                    AddRepo();
                }

                GUILayout.EndHorizontal();

                // TODO: Change to is valid.
                if (Directory.Exists(repository.URI) || Repository.URIChecker.CheckURI(repository.URI) == Repository.URIChecker.URIType.GIT)
                {
                    if (GUILayout.Button("Update"))
                    {
                        repository.ExecuteUpdate(REPO_TEMP_DIR);
                    }

                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    foreach (var package in repository.Packages)
                    {
                        GUILayout.BeginVertical("Box");

                        GUILayout.BeginHorizontal();

                        GUILayout.BeginVertical();
                        GUILayout.Label("Name: " + package.name);
                        GUILayout.Label("Description: " + package.description);

                        if (package.dependencies.Count > 0)
                            GUILayout.Label("Dependencies: " + String.Join(", ", package.dependencies.ToArray()));
                        else
                            GUILayout.Label("No dependencies!");

                        GUILayout.Label("Version: " + package.version);
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        var oldPack = repository.InstalledPackages.FirstOrDefault(str => str.name == package.name);
                        if (oldPack == null)
                        {
                            if (GUILayout.Button("Install"))
                            {
                                repository.InstallPackage(PLUGIN_TEMP_DIRECTORY, PLUGIN_DIRECTORY, package.name);
                                File.WriteAllText(REPO_DIR + "/" + repository.Name + ".json", JsonUtility.ToJson(repository));
                            }
                        }
                        else
                        {
                            if(oldPack.version != package.version)
                            {
                                if (GUILayout.Button("Update Available"))
                                {
                                    repository.InstallPackage(PLUGIN_TEMP_DIRECTORY, PLUGIN_DIRECTORY, package.name);
                                    File.WriteAllText(REPO_DIR + "/" + repository.Name + ".json", JsonUtility.ToJson(repository));
                                }
                            }
                            else
                            {
                                GUILayout.Label("Up to date.");
                            }
                        }

                        GUILayout.EndVertical();


                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUI.color = Color.red;
                    GUILayout.Label("The repository directory doesn't exist.");
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
    }
}

ubPackageManager
=

A **simple** package manager for use with Unity that uses git. The only external tool is git.

Some quick information

* Doesn't support ssh repo access.
* Doesn't support lfs.
* Uses git only and it must be accessible from PATH.
* When selecting the download location for packages you cannot place it within your source for the project.
* It creates a file `Config/PacmanConfig.json` that contains all the repository and package data for the project.

Getting Started
-

0. Download from [*here*](https://bitbucket.org/bifroststudios/ubpackagemanager/downloads/)
1. Once installed in your project you can open the window from: `Bifrost->Package Manager`
2. Select a download directory by clicking the `Change` button, you can use the same directory across multiple projects if you like or a custom one per project. Select it outside of the source control of your current project
3. Add a repository by clicking on `Manage Repositories` button and then clicking `Add Repo`
4. Make sure to `Update` the repo to grab all packages

To create a repo just make a git repository and push up a group of package definition files.

Here is an example of a package definition file, the name of the file must match the name in the file:

```
#!javascript
{
    "name": "ubGridArray",
    "versions": [{
        "version": "1",
        "branch": "version-1"
    }],
    "description": "1D array as 2D array.",
    "location": "https://nhold@bitbucket.org/bifroststudios/ubgridarray.git",
    "parentDir": "Bifrost",
    "childDir": "ubgridarray/Assets/Plugins/ubGridArray"
     "dependencies": [
        "ubConfig"
    ]
}
```

Screenshots
-

![PackageManager.png](https://bitbucket.org/repo/EK6epb/images/4288307946-PackageManager.png)
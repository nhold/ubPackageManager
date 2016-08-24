ubPackageManager
=

A self updating Unity based package manager.

Instructions:
-

0. Download from [*here*](https://bitbucket.org/bifroststudios/ubpackagemanager/downloads/ubPackageManagerV1.0.unitypackage)
1. Add a package repo (Either a directory or git repo)
2. Add a ssh key if some repos are private
3. Click Update Package Repository
4. Install the packages you want!

How to create a local repo
-
1. Create a directory
2. Create a package definition file (.json)

Here is an example:

```
#!javascript
{
    "name": "examplePackage",
    "version": "2.5",
    "description": "It's an example",
    "location": "C:/LocalDirectory/Package/",
    "parentDir": "ExampleNamespace"
    "childDir": "copy/from/this/dir/relative/to/location",
    "dependencies": [
        "someDep"
    ]
}
```

Here is that example lined up with the definition file for this package:

```
#!javascript
{
    "name": "ubPackageManager",
    "version": "1.1",
    "description": "Unity package management.",
    "location": "https://nhold@bitbucket.org/bifroststudios/ubpackagemanager.git",
    "parentDir": "Bifrost",
    "childDir": "ubpackagemanager/Assets/ubPackageManager",
    "dependencies": [
        "ubConfig"
    ]
}
```

Screenshots
-

![PackageManager.png](https://bitbucket.org/repo/EK6epb/images/4288307946-PackageManager.png)

TODO
-
* Better UI (Buttons shouldn't scale)
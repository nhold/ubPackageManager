ubPackageManager
=

A unity based package manager.

Instructions:
-

0. Download from [*here*](TODO)
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
}
```

Screenshots:

![PackageManager.png](https://bitbucket.org/repo/EK6epb/images/4288307946-PackageManager.png)
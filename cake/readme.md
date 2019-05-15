Script for Atlas nuget packages update.

Note! nuget.exe should be in $PATH variable.

### Install cake tool

Under Package Manager Console from VS perform

```
dotnet tool install -g cake.tool
```

### Update Atlas packages

Under Package Manager Console from VS perform

```
dotnet-cake ..\cake\update-atlas-packages.cake
```
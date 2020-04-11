[![Download](https://img.shields.io/nuget/dt/unity-packer)](https://www.nuget.org/packages/unity-packer/)
[![Main](https://github.com/MirrorNG/unity-packer/workflows/Main/badge.svg)](https://github.com/MirrorNG/unity-packer/actions?query=workflow%3AMain)

# unity-packer
unity-packer is a simple dotnet utility to pack and unpack Unity's own UnityPackage file format (usually associated with the .unitypackage extension). It's very useful for CI servers.

The UnityPackage format is not open and everything this utility does is based on reverse engineering the fairly simplistic file format. Thus it might not fit all specifications, as they are not public.

_Note that this is not officially supported or endorsed by Unity. Use at your own risk._

## Installation
Make sure you have [.Net Core 3.1 or later](https://dotnet.microsoft.com/download)

Install unity-packer with the following command:
```sh
dotnet tool install --global unity-packer
```

## Usage

### Packing

To pack some files, use the pack subcommand:
```sh
unity-packer pack <unity package> <inputfile> <targetfile> ...
```

For example, this will create a file `Output.unitypackage` with a file `Assets/Editor/TargetExportPath.cs` with the contents of the local file `MyInputFile.cs`

```sh
unity-packer pack Output.unitypackage MyInputFile.cs Assets/Editor/TargetExportPath.cs```
```

### Unpack

To unpack some files, use the unpack subcommand:

```sh
unity-packer unpack <unity package> <Project Folder>
```

For example, the following command will unpack all the files from `Input.unitypackage` into `Projectfolder`

```sh
./unity-packer unpack Input.unitypackage ProjectFolder
```

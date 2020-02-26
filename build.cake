#load CakeScripts\GAssembly.cake
#load CakeScripts\Settings.cake
#addin "Cake.FileHelpers"
#addin "Cake.Incubator"

Settings.Cake = Context;

var target = Argument("target", "Default");
var configuration = Argument("Configuration", "Release");

var msbuildsettings = new DotNetCoreMSBuildSettings();

Task("Init")
    .Does(() =>
{
    // Add stuff to list
    Settings.Init();
});

Task("Clean")
    .IsDependentOn("Init")
    .Does(() =>
{
    foreach(var gassembly in Settings.AssemblyList)
        gassembly.Clean();
});

Task("FullClean")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DeleteDirectory("BuildOutput", true);
});

Task("Prepare")
    .IsDependentOn("Clean")
    .Does(() =>
{
    // Build tools
    DotNetCoreRestore("Source/Tools/gapi/Gapi.sln");
    DotNetCoreBuild("Source/Tools/gapi/Gapi.sln", new DotNetCoreBuildSettings {
        Verbosity = DotNetCoreVerbosity.Minimal,
        Configuration = configuration,
        OutputDirectory = "BuildOutput/Tools"
    });

    // Generate code and prepare libs projects
    foreach(var gassembly in Settings.AssemblyList)
        gassembly.Prepare();
});

Task("Default")
  .IsDependentOn("Prepare");

RunTarget(target);
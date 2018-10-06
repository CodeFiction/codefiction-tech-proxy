#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.Docker&version=0.9.5"

var target = Argument("target", "Default");
var nugetConfig = Argument("nuget_config", "NuGet.Config");
string dockerImageName = Argument("image_name", string.Empty);

using System;
using System.Diagnostics;

// Variables
var configuration = "Release";
var netCoreTarget = "netcoreapp2.1";

string codefictionProxy = "./src/CodefictionTech.Proxy/CodefictionTech.Proxy.csproj";

Task("Default")
    .IsDependentOn("Docker-Build");

Task("Compile")
    .Description("Builds all the projects in the solution")
    .Does(() =>
    {
        StartProcess("dotnet", new ProcessSettings {
            Arguments = "--info"
        });

        DotNetCoreBuildSettings buildSettings = new DotNetCoreBuildSettings();
        buildSettings.NoRestore = true;
        buildSettings.Configuration = configuration;

        Information("Restoring project");
        DotNetCoreRestore(codefictionProxy);
        Information("Building project");
        DotNetCoreBuild(codefictionProxy, buildSettings);   
    });

Task("Test")
    .Description("Run Tests")
    .IsDependentOn("Compile")
    .Does(() =>
    {
        Information("No Tests :(");
    });

Task("Publish")
    .Description("Run Tests")
    .IsDependentOn("Test")
    .Does(() =>
    {
        DotNetCorePublishSettings publishSettings = new DotNetCorePublishSettings();
        publishSettings.NoRestore = true;
        publishSettings.Configuration = configuration;

        DotNetCorePublish(codefictionProxy, publishSettings);
    });

Task("Docker-Build")
    .IsDependentOn("Publish")
    .Does(() =>
    {
        string publishFolder = $"/bin/{configuration}/{netCoreTarget}/publish";

        Information(dockerImageName);
        Information(publishFolder);

        DockerImageBuildSettings settings = new DockerImageBuildSettings();
        settings.BuildArg = new [] {$"publishFolder={publishFolder}", $"aspnetCoreEnv=Production"};
        settings.WorkingDirectory = "./src/CodefictionTech.Proxy";
        settings.File = "./Dockerfile";
        settings.Tag = new [] {dockerImageName};

        DockerBuild(settings, ".");
    });

RunTarget(target);
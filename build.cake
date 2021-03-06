#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.Docker&version=0.9.5"
#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.AWS.S3&version=0.6.6"

using Path = System.IO.Path;
using System.Text.RegularExpressions;
using System;
using System.Diagnostics;

// Arguments
string target = Argument("target", "Default");
string dockerImageName = Argument("imagename", "cfProxy");
string version = Argument("targetversion", "1.0.0");
string bucketName = Argument("bucketname", "codefiction-tech-proxy-lambda");
bool taintApiDeployment = Argument<bool>("taintapigateway", false);


// Variables
string configuration = "Release";
string netCoreTarget = "netcoreapp2.1";
string packageName = "cfProxy";


// Directories
var cfProxyDir = MakeAbsolute(Directory("./src/CodefictionTech.Proxy"));
var cfProxyPublishDir = cfProxyDir +  Directory($"/bin/{configuration}/{netCoreTarget}/publish");
var terraformDir = MakeAbsolute(Directory("./terraform"));
var awsLambdaToolsDir = MakeAbsolute(Directory("./tools/amazon.lambda.tools/tools/netcoreapp2.1/any"));
string cfProxyProj = cfProxyDir + "/CodefictionTech.Proxy.csproj";

Task("Default")
    .IsDependentOn("Test");

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
        DotNetCoreRestore(cfProxyProj);
        Information("Building project");
        DotNetCoreBuild(cfProxyProj, buildSettings);   
    });

Task("Test")
    .Description("Run Tests")
    .IsDependentOn("Compile")
    .Does(() =>
    {
        Information("No Tests :(");
    });

Task("Publish")
    .Description("Publishes Codefiction Proxy Web")
    .IsDependentOn("Compile")
    .Does(() =>
    {
        DotNetCorePublishSettings publishSettings = new DotNetCorePublishSettings();
        publishSettings.NoRestore = true;
        publishSettings.Configuration = configuration;

        DotNetCorePublish(cfProxyProj, publishSettings);
    });

Task("Package-AwsLambda")
    .Description("Zips up Codefiction proxy web app built binaries for aws lambda")
    .Does(() =>
    {
        string packageNameVersion = GetPackageName();

        var files = GetFiles(cfProxyPublishDir + "/**/*");
        var outputPath = Path.Combine(terraformDir.FullPath, packageNameVersion);

        // TODO : add general cleaning task
        if(FileExists(outputPath))
        {
            Information($"Deleting {packageNameVersion}");
            DeleteFile(outputPath);
        }

        Information($"Zipping {packageNameVersion}");

        StartProcess("dotnet", new ProcessSettings {
            Arguments = $"{awsLambdaToolsDir}/dotnet-lambda.dll package --framework {netCoreTarget} -o {outputPath} -c {configuration}",
            WorkingDirectory = cfProxyDir
        });
    });

Task("Upload-Package-S3")
    .Description("Zips up Codefiction proxy web app built binaries for aws lambda")
    .IsDependentOn("Package-AwsLambda")
    .Does(async () =>
    {
        string packageNameVersion = GetPackageName();
        var packagePath = Path.Combine(terraformDir.FullPath, packageNameVersion);

        await S3Upload(packagePath, packageNameVersion, new UploadSettings()
        {
            AccessKey = EnvironmentVariable("AWS_ACCESS_KEY_ID"),
            SecretKey = EnvironmentVariable("AWS_SECRET_ACCESS_KEY"),

            Region = RegionEndpoint.EUCentral1,
            BucketName = bucketName,

            CannedACL = S3CannedACL.Private
        });
    });

Task("Publish-AwsLambda")
    .Description("Publish zip file to AWS Lamda and configure AWS API Gateway")
    .IsDependentOn("Init-Terraform")
    .IsDependentOn("Upload-Package-S3")
    .Does(() =>
    {
        if(taintApiDeployment)
        {
            StartProcess("terraform", new ProcessSettings {
                Arguments = "taint aws_api_gateway_deployment.cf_proxy_deploy -auto-approve",
                WorkingDirectory = terraformDir
            });
        }

        string packageNameVersion = GetPackageName();
        var packagePath = Path.Combine(terraformDir.FullPath, packageNameVersion);
        Information($"Package to upload : {packagePath}");

        var processSet =  new ProcessSettings() {
                Arguments = $"apply -var bucket_name={bucketName} -var package_name={packageNameVersion} -input=false -auto-approve",
                //Arguments = $"plan -var bucket_name={bucketName} -var package_name={packageNameVersion} -input=false",
                WorkingDirectory = terraformDir
            };

        StartProcess($"terraform", processSet);
    });

Task("Init-Terraform")
    .Does(() =>
    {
        string travis = EnvironmentVariable("TRAVIS");

        if(travis != "true")
        {
            return;
        }

        Information(terraformDir.FullPath);
        StartProcess("terraform", new ProcessSettings {
            Arguments = "init",
            WorkingDirectory = terraformDir
        });
    });

Task("Docker-Build")
    .IsDependentOn("Compile")
    .Does(() =>
    {
        string imageName = GetDockerImageName();
        string dockerFilePath = Path.Combine(cfProxyDir.FullPath, "Dockerfile");

        Information(imageName);
        Information(dockerFilePath);

        DockerImageBuildSettings settings = new DockerImageBuildSettings();
        settings.WorkingDirectory = "./src";
        settings.File = dockerFilePath;
        settings.Tag = new [] {imageName};

        DockerBuild(settings, ".");
    });

Task("Update-Version")
    .Does(() =>
    {
        UpdateProjectVersion(version);
    });

Task("Get-Version")
    .Does(() =>
    {
        string version = GetProjectVersion();

        Information(version);
    });

RunTarget(target);

/*
/ HELPER METHODS
*/
private void UpdateProjectVersion(string version, string csprojPath = null)
{
    Information("Setting version to " + version);

    if(string.IsNullOrWhiteSpace(version))
    {
        throw new CakeException("No version specified! You need to pass in --targetversion=\"x.y.z\"");
    }

    csprojPath = csprojPath ?? cfProxyProj;
    var file =  MakeAbsolute(File(csprojPath));

    Information(file.FullPath);

    var project = System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8);

    var projectVersion = new Regex(@"<Version>.+<\/Version>");
    project = projectVersion.Replace(project, string.Concat("<Version>", version, "</Version>"));

    System.IO.File.WriteAllText(file.FullPath, project, Encoding.UTF8);
}

private string GetProjectVersion(string csprojPath = null)
{
    csprojPath = csprojPath ?? cfProxyProj;
    var file =  MakeAbsolute(File(csprojPath));

    Information(file.FullPath);

    var project = System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8);
    int startIndex = project.IndexOf("<Version>") + "<Version>".Length;
    int endIndex = project.IndexOf("</Version>", startIndex);

    string version = project.Substring(startIndex, endIndex - startIndex);
    string buildNumber = (EnvironmentVariable("TRAVIS_BUILD_NUMBER")) ?? "0";
    version = $"{version}.{buildNumber}";

    return version;
}

private string GetPackageName()
{
    return $"{packageName}-{GetProjectVersion()}.zip";     
}

private string GetDockerImageName()
{
    return $"{dockerImageName.ToLowerInvariant()}-{configuration.ToLowerInvariant()}:{GetProjectVersion()}";
}
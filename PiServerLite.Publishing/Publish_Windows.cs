﻿using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using Photon.Framework.Tools;
using Photon.MSBuild;
using Photon.NuGetPlugin;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PiServerLite.Publishing
{
    public class Publish_Windows : IBuildTask
    {
        public IAgentBuildContext Context {get; set;}

        
        public async Task RunAsync(CancellationToken token)
        {
            await BuildSolution(token);
            await PublishProjectPackage(token);
        }

        private async Task BuildSolution(CancellationToken token)
        {
            var msbuild = new MSBuildCommand(Context) {
                Exe = Context.AgentVariables["global"]["msbuild_exe"],
                WorkingDirectory = Context.ContentDirectory,
            };

            var buildArgs = new MSBuildArguments {
                ProjectFile = "PiServerLite.sln",
                Properties = {
                    ["Configuration"] = "Release",
                    ["Platform"] = "Any CPU",
                },
                Verbosity = MSBuildVerbosityLevel.Minimal,
                MaxCpuCount = 0,
            };

            await msbuild.RunAsync(buildArgs, token);
        }

        private async Task PublishProjectPackage(CancellationToken token)
        {
            var projectPath = Path.Combine(Context.ContentDirectory, "PiServerLite");
            var packageDefinition = Path.Combine(projectPath, "PiServerLite.csproj");
            var assemblyFilename = Path.Combine(projectPath, "bin", "Release", "PiServerLite.dll");
            var assemblyVersion = AssemblyTools.GetVersion(assemblyFilename);

            await PublishPackage("PiServerLite", packageDefinition, assemblyVersion, token);
        }

        private async Task PublishPackage(string packageId, string packageDefinitionFilename, string assemblyVersion, CancellationToken token)
        {
            var nugetPackageDir = Path.Combine(Context.WorkDirectory, "Packages");
            var nugetApiKey = Context.ServerVariables["global"]["nuget.apiKey"];
            var nugetExe = Path.Combine(Context.ContentDirectory, "bin", "NuGet.exe");

            var publisher = new NuGetPackagePublisher(Context) {
                Mode = NugetModes.CommandLine,
                PackageDirectory = nugetPackageDir,
                PackageDefinition = packageDefinitionFilename,
                PackageId = packageId,
                Version = assemblyVersion,
                CL = new NuGetCommandLine {
                    ExeFilename = nugetExe,
                    ApiKey = nugetApiKey,
                    Output = Context.Output,
                },
                //Client = new NuGetCore {
                //    EnableV3 = true,
                //    ApiKey = nugetApiKey,
                //    Output = Context.Output,
                //},
                PackProperties = {
                    ["Configuration"] = "Release",
                    ["Platform"] = "AnyCPU",
                },
            };

            //publisher.Client.Initialize();

            await publisher.PublishAsync(token);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;

namespace NgSdms.Master.Generator
{
    public partial class ProjectGenerator
    {
        private readonly string _solutionName;
        private readonly string _outputPath;
        private readonly string _projectName;
        private readonly string _projectDescription;
        private readonly bool _includeDockerSupport;
        private readonly Dictionary<string, string> _dependencies;

        public ProjectGenerator(
            string solutionName,
            string outputPath,
            string projectName,
            string projectDescription,
            bool includeDockerSupport,
            Dictionary<string, string>? dependencies = null)
        {
            _solutionName = solutionName;
            _outputPath = outputPath;
            _projectName = projectName;
            _projectDescription = projectDescription;
            _includeDockerSupport = includeDockerSupport;
            _dependencies = dependencies ?? new Dictionary<string, string>();
        }

        public void Generate()
        {
            Console.WriteLine($"Generating project {_projectName}...");

            // Create solution directory
            var solutionDir = Path.Combine(_outputPath, _solutionName);
            Directory.CreateDirectory(solutionDir);

            // Create solution file
            CreateSolutionFile(solutionDir);

            // Create project structure
            CreateProjectStructure(solutionDir);

            // Install dependencies
            InstallDependencies(solutionDir);

            Console.WriteLine($"Project generation completed for {_projectName}");
        }

        private void CreateSolutionFile(string solutionDir)
        {
            Console.WriteLine("Creating solution file...");
            var slnPath = Path.Combine(solutionDir, $"{_solutionName}.sln");

            // Check if solution already exists
            if (!File.Exists(slnPath))
            {
                // Create new solution
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"new sln -n {_solutionName}",
                    WorkingDirectory = solutionDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(startInfo);
                process?.WaitForExit();

                if (process?.ExitCode != 0)
                {
                    Console.Error.WriteLine($"Error creating solution file: {process?.StandardError.ReadToEnd()}");
                    return;
                }

                Console.WriteLine($"Solution file created: {slnPath}");
            }
            else
            {
                Console.WriteLine($"Solution file already exists: {slnPath}");
            }
        }

        private void CreateProjectStructure(string solutionDir)
        {
            Console.WriteLine("Creating project structure...");

            // Create API project
            var apiProjectDir = Path.Combine(solutionDir, $"{_projectName}.Api");
            CreateApiProject(apiProjectDir);

            // Create Domain project (Management)
            var domainProjectDir = Path.Combine(solutionDir, $"{_projectName}.Management");
            CreateDomainProject(domainProjectDir);
            
            // Create Application project (Federation)
            var applicationProjectDir = Path.Combine(solutionDir, $"{_projectName}.Federation");
            CreateApplicationProject(applicationProjectDir);
            
            // Create Infrastructure project
            var infrastructureProjectDir = Path.Combine(solutionDir, $"{_projectName}.Infrastructure");
            CreateInfrastructureProject(infrastructureProjectDir);

            // Add projects to solution
            AddProjectToSolution(solutionDir, apiProjectDir, $"{_projectName}.Api");
            AddProjectToSolution(solutionDir, domainProjectDir, $"{_projectName}.Management");
            AddProjectToSolution(solutionDir, applicationProjectDir, $"{_projectName}.Federation");
            AddProjectToSolution(solutionDir, infrastructureProjectDir, $"{_projectName}.Infrastructure");

            // Add project references
            AddProjectReferences(apiProjectDir, applicationProjectDir, infrastructureProjectDir);
            AddProjectReferences(applicationProjectDir, domainProjectDir, infrastructureProjectDir);
            AddProjectReferences(infrastructureProjectDir, domainProjectDir);
        }

        partial void CreateApiProject(string projectDir);

        partial void CreateDomainProject(string projectDir);

        partial void CreateApplicationProject(string projectDir);

        partial void CreateInfrastructureProject(string projectDir);
        // Partial methods for Auth scaffolding and Dockerfile generation
        partial void CreateKeycloakAuthExtensions(string projectDir);
        partial void CreateDockerfile(string projectDir);

        private void AddProjectToSolution(string solutionDir, string projectDir, string projectName)
        {
            // Build the full project file path using correct path separators for the OS
            var projectFilePath = Path.Combine(projectDir, projectName + ".csproj");
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"sln add \"{projectFilePath}\"",
                WorkingDirectory = solutionDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            process?.WaitForExit();

            if (process?.ExitCode != 0)
            {
                Console.Error.WriteLine($"Error adding project to solution: {process?.StandardError.ReadToEnd()}");
            }
        }

        private void AddProjectReferences(string projectDir, params string[] referencedProjects)
        {
            foreach (var referencedProject in referencedProjects)
            {
                var projectName = Path.GetFileName(referencedProject);
                string referencePath = Path.Combine(referencedProject, $"{Path.GetFileName(referencedProject)}.csproj");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"add \"{projectDir}\" reference \"{referencePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(startInfo);
                process?.WaitForExit();

                if (process?.ExitCode != 0)
                {
                    Console.Error.WriteLine($"Error adding project reference: {process?.StandardError.ReadToEnd()}");
                }
            }
        }

        private void InstallDependencies(string solutionDir)
        {
            Console.WriteLine("Installing dependencies...");

            // Domain (Management) project packages
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Management"), "Microsoft.EntityFrameworkCore.SqlServer", "9.0.7");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Management"), "Microsoft.EntityFrameworkCore.Tools", "9.0.7");

            // Application (Federation) project packages
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Federation"), "AutoMapper", "12.0.1");

            // API project packages (matching NgSdms.Master.Api.csproj)
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Api"), "Microsoft.AspNetCore.Authentication.JwtBearer", "8.0.7");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Api"), "Swashbuckle.AspNetCore", "6.6.2");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Api"), "Microsoft.VisualStudio.Azure.Containers.Tools.Targets", "1.20.1");

            // Infrastructure project packages (matching NgSdms.Master.Infrastructure.csproj)
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Infrastructure"), "AutoMapper", "12.0.1");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Infrastructure"), "AutoMapper.Extensions.Microsoft.DependencyInjection", "12.0.1");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Infrastructure"), "Microsoft.AspNetCore.Http.Abstractions", "2.3.0");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Infrastructure"), "Microsoft.EntityFrameworkCore", "9.0.7");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Infrastructure"), "Microsoft.EntityFrameworkCore.SqlServer", "9.0.7");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Infrastructure"), "Microsoft.Extensions.Configuration.Abstractions", "9.0.7");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Infrastructure"), "Microsoft.Extensions.DependencyInjection.Abstractions", "9.0.7");
            InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Infrastructure"), "Npgsql.EntityFrameworkCore.PostgreSQL", "9.0.4");

            // Install any custom dependencies
            foreach (var dependency in _dependencies)
            {
                InstallPackage(Path.Combine(solutionDir, $"{_projectName}.Api"), dependency.Key, dependency.Value);
            }
        }

        private void InstallPackage(string projectDir, string packageName, string version)
        {
            Console.WriteLine($"Installing {packageName} {version}...");

            // Make sure the directory exists
            if (!Directory.Exists(projectDir))
            {
                Console.WriteLine($"Warning: Directory {projectDir} does not exist, skipping package installation.");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"add package {packageName} --version {version}",
                WorkingDirectory = projectDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            process?.WaitForExit();

            if (process?.ExitCode != 0)
            {
                Console.Error.WriteLine($"Error installing package {packageName}: {process?.StandardError.ReadToEnd()}");
            }
        }

        private void RunDotNetCommand(string command, string workingDir)
        {
            // Make sure the directory exists
            if (!Directory.Exists(workingDir))
            {
                Directory.CreateDirectory(workingDir);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = command,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            process?.WaitForExit();

            if (process?.ExitCode != 0)
            {
                Console.Error.WriteLine($"Error running command '{command}': {process?.StandardError.ReadToEnd()}");
            }
        }
    }
}

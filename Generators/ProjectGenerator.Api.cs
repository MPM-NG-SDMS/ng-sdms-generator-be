using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NgSdms.Master.Generator
{
    public partial class ProjectGenerator
    {
        // Implementation of CreateApiProject moved here
        partial void CreateApiProject(string projectDir)
        {
            Directory.CreateDirectory(projectDir);

            // Create API project
            RunDotNetCommand($"new webapi -n {_projectName}.Api -f net8.0 --force -o .", projectDir);

            // Create folders
            Directory.CreateDirectory(Path.Combine(projectDir, "Controllers"));
            Directory.CreateDirectory(Path.Combine(projectDir, "Auth"));
            Directory.CreateDirectory(Path.Combine(projectDir, "Middleware"));
            Directory.CreateDirectory(Path.Combine(projectDir, "Extensions"));

            // Appsettings and Program.cs scaffolding
            // ...original code from CreateApiProject...
        }
    }
}

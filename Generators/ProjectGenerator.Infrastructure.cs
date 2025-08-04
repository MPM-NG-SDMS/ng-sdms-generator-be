using System;
using System.IO;
using System.Text;

namespace NgSdms.Master.Generator
{
    public partial class ProjectGenerator
    {
        // Implementation of CreateInfrastructureProject moved here
        partial void CreateInfrastructureProject(string projectDir)
        {
            Directory.CreateDirectory(projectDir);
            RunDotNetCommand($"new classlib -n {_projectName}.Infrastructure -f net8.0 --force -o .", projectDir);
            // original code for scaffolding Infrastructure project...
        }
    }
}

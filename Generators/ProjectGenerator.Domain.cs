using System;
using System.IO;
using System.Text;

namespace NgSdms.Master.Generator
{
    public partial class ProjectGenerator
    {
        // Implementation of CreateDomainProject moved here
        partial void CreateDomainProject(string projectDir)
        {
            Directory.CreateDirectory(projectDir);
            RunDotNetCommand($"new classlib -n {_projectName}.Management -f net8.0 --force -o .", projectDir);
            // original code for scaffolding Domain project...
        }
    }
}

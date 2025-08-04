using System;
using System.IO;
using System.Text;

namespace NgSdms.Master.Generator
{
    public partial class ProjectGenerator
    {
        // Implementation of CreateApplicationProject moved here
        partial void CreateApplicationProject(string projectDir)
        {
            Directory.CreateDirectory(projectDir);
            RunDotNetCommand($"new classlib -n {_projectName}.Federation -f net8.0 --force -o .", projectDir);
            // original code for scaffolding Application project...
        }
    }
}

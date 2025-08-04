using System;
using System.IO;
using System.Text;

namespace NgSdms.Master.Generator
{
    public partial class ProjectGenerator
    {
        // Implementation of CreateDockerfile moved here
        partial void CreateDockerfile(string projectDir)
        {
            var projectName = new DirectoryInfo(projectDir).Name;
            var dockerfileContent = new StringBuilder();
            dockerfileContent.AppendLine("# Stage 1: Build");
            dockerfileContent.AppendLine("FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build");
            dockerfileContent.AppendLine("WORKDIR /src");
            dockerfileContent.AppendLine($"COPY ./{projectName}.Api/{projectName}.Api.csproj ./");
            dockerfileContent.AppendLine("RUN dotnet restore");
            dockerfileContent.AppendLine($"COPY . ./{projectName}.Api/");
            dockerfileContent.AppendLine("RUN dotnet publish -c Release -o /app --no-restore");
            dockerfileContent.AppendLine();
            dockerfileContent.AppendLine("# Stage 2: Runtime");
            dockerfileContent.AppendLine("FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime");
            dockerfileContent.AppendLine("WORKDIR /app");
            dockerfileContent.AppendLine("COPY --from=build /app .");
            dockerfileContent.AppendLine("ENTRYPOINT [ \"dotnet\", \"Project.Api.dll\" ]");

            File.WriteAllText(Path.Combine(projectDir, "Dockerfile"), dockerfileContent.ToString());
        }
    }
}

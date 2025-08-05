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

            // Scaffold sample Product entity
            var entitiesDir = Path.Combine(projectDir, "Entities");
            var productEntity = new StringBuilder();
            productEntity.AppendLine("using System;");
            productEntity.AppendLine();
            productEntity.AppendLine($"namespace {_projectName}.Management.Entities");
            productEntity.AppendLine("{");
            productEntity.AppendLine("    public class Product");
            productEntity.AppendLine("    {");
            productEntity.AppendLine("        public Guid Id { get; set; }");
            productEntity.AppendLine("        public string Name { get; set; } = string.Empty;");
            productEntity.AppendLine("        public decimal Price { get; set; }");
            productEntity.AppendLine("    }");
            productEntity.AppendLine("}");
            File.WriteAllText(Path.Combine(entitiesDir, "Product.cs"), productEntity.ToString());

            // Scaffold IProductRepository
            var repoInterfacesDir = Path.Combine(projectDir, "Repositories", "Interfaces");
            var productRepo = new StringBuilder();
            productRepo.AppendLine($"namespace {_projectName}.Management.Repositories.Interfaces");
            productRepo.AppendLine("{");
            productRepo.AppendLine("    public interface IProductRepository : IGenericRepository<Product>");
            productRepo.AppendLine("    {");
            productRepo.AppendLine("    }");
            productRepo.AppendLine("}");
            File.WriteAllText(Path.Combine(repoInterfacesDir, "IProductRepository.cs"), productRepo.ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NgSdms.Master.Generator;

namespace NgSdms.Master.Generator
{
    // Configuration model for the generator
    public class GeneratorConfig
    {
        public string BasePath { get; set; } = string.Empty;
        public string SolutionName { get; set; } = "NgSdms.Master";
        public string ProjectName { get; set; } = "NgSdms.Master";
        public string ProjectDescription { get; set; } = "NG SDMS Master Data Management";
        public bool IncludeDockerSupport { get; set; } = true;
        public Dictionary<string, string> Dependencies { get; set; } = new Dictionary<string, string>();
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Load configuration from config.json
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            if (!File.Exists(configFile))
            {
                Console.Error.WriteLine($"Config file not found: {configFile}");
                return;
            }
            var configJson = File.ReadAllText(configFile);
            var config = JsonSerializer.Deserialize<GeneratorConfig>(configJson);
            if (config == null || string.IsNullOrWhiteSpace(config.BasePath))
            {
                Console.Error.WriteLine("Invalid configuration in config.json");
                return;
            }

            // Get command line arguments
            if (args.Length == 0)
            {
                Console.WriteLine("Available commands:");
                Console.WriteLine("  project - Create a new project structure");
                Console.WriteLine("  entity - Generate entity and related files");
                Console.WriteLine("  all - Create project and generate entities");
                return;
            }

            string command = args[0].ToLower();

            if (command == "project" || command == "all")
            {
                // Generate project structure
                Console.WriteLine("Generating project structure...");
                var projectGenerator = new ProjectGenerator(
                    config.SolutionName,
                    config.BasePath,
                    config.ProjectName,
                    config.ProjectDescription,
                    config.IncludeDockerSupport,
                    config.Dependencies
                );
                projectGenerator.Generate();
                Console.WriteLine("Project structure generated successfully!");
            }

            if (command == "entity" || command == "all")
            {
                // Example: Create entities
                GenerateEntities(config.BasePath);
            }

            Console.WriteLine("Generation completed successfully!");
        }

        private static void GenerateEntities(string basePath)
        {
            Console.WriteLine("Generating entities...");

            // Example: Create a new Product entity
            var productProperties = new List<PropertyDefinition>
            {
                new PropertyDefinition("Code", "code", "string", false, 64),
                new PropertyDefinition("Name", "name", "string", false, 100),
                new PropertyDefinition("Description", "description", "string", true, 500),
                new PropertyDefinition("Price", "price", "decimal", false),
                new PropertyDefinition("IsActive", "isactive", "bool", true)
            };
            
            var productGenerator = new EntityGenerator(basePath, "Mstproduct", "mstproduct", productProperties);
            productGenerator.Generate();
            
            // Example: Create a new Category entity
            var categoryProperties = new List<PropertyDefinition>
            {
                new PropertyDefinition("Code", "code", "string", false, 64),
                new PropertyDefinition("Name", "name", "string", false, 100),
                new PropertyDefinition("Description", "description", "string", true, 500),
                new PropertyDefinition("SortOrder", "sortorder", "int", true)
            };
            
            var categoryGenerator = new EntityGenerator(basePath, "Mstcategory", "mstcategory", categoryProperties);
            categoryGenerator.Generate();
        }
    }
}

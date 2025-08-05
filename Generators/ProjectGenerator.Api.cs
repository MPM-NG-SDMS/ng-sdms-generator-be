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

            // Scaffold additional controllers
            var controllersDir = Path.Combine(projectDir, "Controllers");
            // HealthController
            var healthController = new StringBuilder();
            healthController.AppendLine("using Microsoft.AspNetCore.Mvc;");
            healthController.AppendLine();
            healthController.AppendLine("namespace " + _projectName + ".Api.Controllers");
            healthController.AppendLine("{");
            healthController.AppendLine("    [ApiController]");
            healthController.AppendLine("    [Route(\"api/[controller]\")]");
            healthController.AppendLine("    public class HealthController : ControllerBase");
            healthController.AppendLine("    {");
            healthController.AppendLine("        [HttpGet]");
            healthController.AppendLine("        public IActionResult Get() => Ok(new { status = \"Healthy\" });");
            healthController.AppendLine("    }");
            healthController.AppendLine("}");
            File.WriteAllText(Path.Combine(controllersDir, "HealthController.cs"), healthController.ToString());

            // AuthController stub
            var authController = new StringBuilder();
            authController.AppendLine("using Microsoft.AspNetCore.Mvc;");
            authController.AppendLine();
            authController.AppendLine("namespace " + _projectName + ".Api.Controllers");
            authController.AppendLine("{");
            authController.AppendLine("    [ApiController]");
            authController.AppendLine("    [Route(\"api/[controller]\")]");
            authController.AppendLine("    public class AuthController : ControllerBase");
            authController.AppendLine("    {");
            authController.AppendLine("        [HttpGet(\"public\")]");
            authController.AppendLine("        public IActionResult Public() => Ok(\"Public endpoint\");");
            authController.AppendLine();
            authController.AppendLine("        [HttpGet(\"secure\")]");
            authController.AppendLine("        public IActionResult Secure() => Ok(\"Secure endpoint\");");
            authController.AppendLine("    }");
            authController.AppendLine("}");
            File.WriteAllText(Path.Combine(controllersDir, "AuthController.cs"), authController.ToString());

            // Scaffold ProductController
            var controller = new StringBuilder();
            controller.AppendLine("using Microsoft.AspNetCore.Mvc;");
            controller.AppendLine();
            controller.AppendLine("namespace " + _projectName + ".Api.Controllers");
            controller.AppendLine("{");
            controller.AppendLine("    [ApiController]");
            controller.AppendLine("    [Route(\"api/[controller]\")]");
            controller.AppendLine("    public class ProductController : ControllerBase");
            controller.AppendLine("    {");
            controller.AppendLine("        private readonly IProductService _service;");
            controller.AppendLine();
            controller.AppendLine("        public ProductController(IProductService service)");
            controller.AppendLine("        {");
            controller.AppendLine("            _service = service;");
            controller.AppendLine("        }");
            controller.AppendLine();
            controller.AppendLine("        [HttpGet]");
            controller.AppendLine("        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());");
            controller.AppendLine();
            controller.AppendLine("        [HttpGet(\"{id}\")]");
            controller.AppendLine("        public async Task<IActionResult> Get(Guid id) => Ok(await _service.GetByIdAsync(id));");
            controller.AppendLine();
            controller.AppendLine("        [HttpPost]");
            controller.AppendLine("        public async Task<IActionResult> Post([FromBody] ProductDto dto) => Ok(await _service.CreateAsync(dto));");
            controller.AppendLine();
            controller.AppendLine("        [HttpPut]");
            controller.AppendLine("        public async Task<IActionResult> Put([FromBody] ProductDto dto)");
            controller.AppendLine("        {");
            controller.AppendLine("            await _service.UpdateAsync(dto);");
            controller.AppendLine("            return NoContent();");
            controller.AppendLine("        }");
            controller.AppendLine();
            controller.AppendLine("        [HttpDelete(\"{id}\")]");
            controller.AppendLine("        public async Task<IActionResult> Delete(Guid id)");
            controller.AppendLine("        {");
            controller.AppendLine("            await _service.DeleteAsync(id);");
            controller.AppendLine("            return NoContent();");
            controller.AppendLine("        }");
            controller.AppendLine("    }");
            controller.AppendLine("}");
            File.WriteAllText(Path.Combine(controllersDir, "ProductController.cs"), controller.ToString());
        }
    }
}

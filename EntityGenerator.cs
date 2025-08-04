using System;
using System.Text.Json;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace NgSdms.Master.Generator
{
    public class EntityGenerator
    {
        private readonly string _basePath;
        private readonly string _entityName;
        private readonly string _entityNameLower;
        private readonly string _tableName;
        private readonly List<PropertyDefinition> _properties;

        public EntityGenerator(string basePath, string entityName, string tableName, List<PropertyDefinition> properties)
        {
            _basePath = basePath;
            _entityName = entityName;
            _entityNameLower = entityName.ToLower();
            _tableName = tableName;
            _properties = properties;
        }

        public void Generate()
        {
            Console.WriteLine($"Generating entity {_entityName}...");
            
            // Ensure directories exist
            EnsureDirectories();
            
            // Generate all components
            GenerateEntity();
            GenerateDto();
            GenerateRepositoryInterface();
            GenerateRepository();
            GenerateServiceInterface();
            GenerateService();
            GenerateController();
            
            Console.WriteLine($"Generation completed for {_entityName}");
        }

        private void EnsureDirectories()
        {
            // Create entity directory
            Directory.CreateDirectory(Path.Combine(_basePath, "NgSdms.Master.Management", "Entities"));
            
            // Create DTO directory
            Directory.CreateDirectory(Path.Combine(_basePath, "NgSdms.Master.Federation", "DTOs"));
            
            // Create repository interface directory
            Directory.CreateDirectory(Path.Combine(_basePath, "NgSdms.Master.Management", "Repositories", "Interfaces"));
            
            // Create repository implementation directory
            Directory.CreateDirectory(Path.Combine(_basePath, "NgSdms.Master.Management", "Repositories", "Implementation"));
            
            // Create service interface directory
            Directory.CreateDirectory(Path.Combine(_basePath, "NgSdms.Master.Federation", "Services", "Interfaces"));
            
            // Create service implementation directory
            Directory.CreateDirectory(Path.Combine(_basePath, "NgSdms.Master.Federation", "Services", "Implementation"));
            
            // Create controller directory
            Directory.CreateDirectory(Path.Combine(_basePath, "NgSdms.Master.API", "Controllers"));
        }

        private void GenerateEntity()
        {
            var entityContent = new StringBuilder();
            entityContent.AppendLine("using System;");
            entityContent.AppendLine("using System.Collections.Generic;");
            entityContent.AppendLine("using System.ComponentModel.DataAnnotations;");
            entityContent.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            entityContent.AppendLine("using Microsoft.EntityFrameworkCore;");
            entityContent.AppendLine();
            entityContent.AppendLine("namespace NgSdms.Master.Management.Entities;");
            entityContent.AppendLine();
            entityContent.AppendLine($"[Table(\"{_tableName}\")]");
            entityContent.AppendLine($"public partial class {_entityName}");
            entityContent.AppendLine("{");
            
            // Add Id property
            entityContent.AppendLine("    [Key]");
            entityContent.AppendLine("    [Column(\"id\")]");
            entityContent.AppendLine("    public Guid Id { get; set; }");
            entityContent.AppendLine();
            
            // Add all properties
            foreach (var property in _properties)
            {
                entityContent.AppendLine($"    [Column(\"{property.ColumnName.ToLower()}\")]");
                
                if (property.MaxLength > 0 && (property.Type == "string"))
                {
                    entityContent.AppendLine($"    [StringLength({property.MaxLength})]");
                    entityContent.AppendLine("    [Unicode(false)]");
                }
                
                if (property.Type == "DateTime")
                {
                    entityContent.AppendLine("    [Column(TypeName = \"datetime\")]");
                }
                
                var nullable = property.IsNullable ? "?" : "";
                var defaultValue = property.Type == "string" && !property.IsNullable ? " = null!;" : ";";
                
                entityContent.AppendLine($"    public {property.Type}{nullable} {property.Name} {{ get; set; }}{defaultValue}");
                entityContent.AppendLine();
            }
            
            // Add audit fields
            entityContent.AppendLine("    [Column(\"createdby\")]");
            entityContent.AppendLine("    [StringLength(32)]");
            entityContent.AppendLine("    [Unicode(false)]");
            entityContent.AppendLine("    public string? Createdby { get; set; }");
            entityContent.AppendLine();
            
            entityContent.AppendLine("    [Column(\"createddate\", TypeName = \"datetime\")]");
            entityContent.AppendLine("    public DateTime? Createddate { get; set; }");
            entityContent.AppendLine();
            
            entityContent.AppendLine("    [Column(\"modifiedby\")]");
            entityContent.AppendLine("    [StringLength(32)]");
            entityContent.AppendLine("    [Unicode(false)]");
            entityContent.AppendLine("    public string? Modifiedby { get; set; }");
            entityContent.AppendLine();
            
            entityContent.AppendLine("    [Column(\"modifieddate\", TypeName = \"datetime\")]");
            entityContent.AppendLine("    public DateTime? Modifieddate { get; set; }");
            entityContent.AppendLine();
            
            entityContent.AppendLine("    [Column(\"deletedby\")]");
            entityContent.AppendLine("    [StringLength(32)]");
            entityContent.AppendLine("    [Unicode(false)]");
            entityContent.AppendLine("    public string? Deletedby { get; set; }");
            entityContent.AppendLine();
            
            entityContent.AppendLine("    [Column(\"deleteddate\", TypeName = \"datetime\")]");
            entityContent.AppendLine("    public DateTime? Deleteddate { get; set; }");
            
            entityContent.AppendLine("}");
            
            File.WriteAllText(Path.Combine(_basePath, "NgSdms.Master.Management", "Entities", $"{_entityName}.cs"), entityContent.ToString());
            Console.WriteLine($"Generated Entity: {_entityName}.cs");
        }

        private void GenerateDto()
        {
            var dtoContent = new StringBuilder();
            dtoContent.AppendLine("using System;");
            dtoContent.AppendLine();
            dtoContent.AppendLine("namespace NgSdms.Master.Federation.DTOs");
            dtoContent.AppendLine("{");
            dtoContent.AppendLine($"    public class {_entityName}Dto : BaseDto");
            dtoContent.AppendLine("    {");
            
            // Add all properties except audit fields
            foreach (var property in _properties)
            {
                var nullable = property.IsNullable ? "?" : "";
                var defaultValue = property.Type == "string" && !property.IsNullable ? " = string.Empty;" : ";";
                
                dtoContent.AppendLine($"        public {property.Type}{nullable} {property.Name} {{ get; set; }}{defaultValue}");
            }
            
            dtoContent.AppendLine("    }");
            dtoContent.AppendLine("}");
            
            File.WriteAllText(Path.Combine(_basePath, "NgSdms.Master.Federation", "DTOs", $"{_entityName}Dto.cs"), dtoContent.ToString());
            Console.WriteLine($"Generated DTO: {_entityName}Dto.cs");
        }

        private void GenerateRepositoryInterface()
        {
            var interfaceContent = new StringBuilder();
            interfaceContent.AppendLine("using System;");
            interfaceContent.AppendLine("using System.Collections.Generic;");
            interfaceContent.AppendLine("using System.Threading.Tasks;");
            interfaceContent.AppendLine($"using NgSdms.Master.Management.Entities;");
            interfaceContent.AppendLine();
            interfaceContent.AppendLine("namespace NgSdms.Master.Management.Interfaces");
            interfaceContent.AppendLine("{");
            interfaceContent.AppendLine($"    public interface I{_entityName}Repository : IGenericRepository<{_entityName}>");
            interfaceContent.AppendLine("    {");
            
            // Common repository methods
            interfaceContent.AppendLine($"        Task<{_entityName}?> GetByCodeAsync(string code);");
            
            // Add name-based search if the entity has Name property
            if (_properties.Any(p => p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)))
            {
                interfaceContent.AppendLine($"        Task<IEnumerable<{_entityName}>> GetByNameAsync(string name);");
            }
            
            interfaceContent.AppendLine("    }");
            interfaceContent.AppendLine("}");
            
            File.WriteAllText(Path.Combine(_basePath, "NgSdms.Master.Management", "Repositories", "Interfaces", $"I{_entityName}Repository.cs"), interfaceContent.ToString());
            Console.WriteLine($"Generated Repository Interface: I{_entityName}Repository.cs");
        }

        private void GenerateRepository()
        {
            var repositoryContent = new StringBuilder();
            repositoryContent.AppendLine("using System;");
            repositoryContent.AppendLine("using System.Collections.Generic;");
            repositoryContent.AppendLine("using System.Linq;");
            repositoryContent.AppendLine("using System.Threading.Tasks;");
            repositoryContent.AppendLine("using Microsoft.EntityFrameworkCore;");
            repositoryContent.AppendLine("using NgSdms.Master.Management.Data;");
            repositoryContent.AppendLine("using NgSdms.Master.Management.Entities;");
            repositoryContent.AppendLine("using NgSdms.Master.Management.Interfaces;");
            repositoryContent.AppendLine();
            repositoryContent.AppendLine("namespace NgSdms.Master.Management.Repositories.Implementation");
            repositoryContent.AppendLine("{");
            repositoryContent.AppendLine($"    public class {_entityName}Repository : GenericRepository<{_entityName}>, I{_entityName}Repository");
            repositoryContent.AppendLine("    {");
            repositoryContent.AppendLine($"        public {_entityName}Repository(NgSdmsMasterDbContext context) : base(context)");
            repositoryContent.AppendLine("        {");
            repositoryContent.AppendLine("        }");
            repositoryContent.AppendLine();
            
            // Implementation of GetByCodeAsync
            repositoryContent.AppendLine($"        public async Task<{_entityName}?> GetByCodeAsync(string code)");
            repositoryContent.AppendLine("        {");
            repositoryContent.AppendLine("            return await _dbSet");
            repositoryContent.AppendLine("                .FirstOrDefaultAsync(e => e.Code == code);");
            repositoryContent.AppendLine("        }");
            
            // Add name-based search if the entity has Name property
            if (_properties.Any(p => p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)))
            {
                repositoryContent.AppendLine();
                repositoryContent.AppendLine($"        public async Task<IEnumerable<{_entityName}>> GetByNameAsync(string name)");
                repositoryContent.AppendLine("        {");
                repositoryContent.AppendLine("            return await _dbSet");
                repositoryContent.AppendLine("                .Where(e => e.Name != null && e.Name.Contains(name))");
                repositoryContent.AppendLine("                .ToListAsync();");
                repositoryContent.AppendLine("        }");
            }
            
            repositoryContent.AppendLine("    }");
            repositoryContent.AppendLine("}");
            
            File.WriteAllText(Path.Combine(_basePath, "NgSdms.Master.Management", "Repositories", "Implementation", $"{_entityName}Repository.cs"), repositoryContent.ToString());
            Console.WriteLine($"Generated Repository: {_entityName}Repository.cs");
        }

        private void GenerateServiceInterface()
        {
            var interfaceContent = new StringBuilder();
            interfaceContent.AppendLine("using System;");
            interfaceContent.AppendLine("using System.Collections.Generic;");
            interfaceContent.AppendLine("using System.Threading.Tasks;");
            interfaceContent.AppendLine($"using NgSdms.Master.Federation.DTOs;");
            interfaceContent.AppendLine();
            interfaceContent.AppendLine("namespace NgSdms.Master.Federation.Services.Interfaces");
            interfaceContent.AppendLine("{");
            interfaceContent.AppendLine($"    public interface I{_entityName}Service");
            interfaceContent.AppendLine("    {");
            interfaceContent.AppendLine("        // CRUD operations");
            interfaceContent.AppendLine($"        Task<{_entityName}Dto?> GetByIdAsync(Guid id);");
            interfaceContent.AppendLine($"        Task<IEnumerable<{_entityName}Dto>> GetAllAsync();");
            interfaceContent.AppendLine($"        Task<{_entityName}Dto> CreateAsync({_entityName}Dto {_entityNameLower}Dto, string createdBy);");
            interfaceContent.AppendLine($"        Task<{_entityName}Dto?> UpdateAsync({_entityName}Dto {_entityNameLower}Dto, string modifiedBy);");
            interfaceContent.AppendLine("        Task<bool> DeleteAsync(Guid id, string deletedBy);");
            interfaceContent.AppendLine("        Task<bool> SoftDeleteAsync(Guid id, string deletedBy);");
            interfaceContent.AppendLine("        ");
            interfaceContent.AppendLine("        // Additional operations");
            interfaceContent.AppendLine($"        Task<{_entityName}Dto?> GetByCodeAsync(string code);");
            
            // Add name-based search if the entity has Name property
            if (_properties.Any(p => p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)))
            {
                interfaceContent.AppendLine($"        Task<IEnumerable<{_entityName}Dto>> GetByNameContainingAsync(string name);");
            }
            
            interfaceContent.AppendLine("    }");
            interfaceContent.AppendLine("}");
            
            File.WriteAllText(Path.Combine(_basePath, "NgSdms.Master.Federation", "Services", "Interfaces", $"I{_entityName}Service.cs"), interfaceContent.ToString());
            Console.WriteLine($"Generated Service Interface: I{_entityName}Service.cs");
        }

        private void GenerateService()
        {
            var serviceContent = new StringBuilder();
            serviceContent.AppendLine("using System;");
            serviceContent.AppendLine("using System.Collections.Generic;");
            serviceContent.AppendLine("using System.Linq;");
            serviceContent.AppendLine("using System.Threading.Tasks;");
            serviceContent.AppendLine("using AutoMapper;");
            serviceContent.AppendLine("using NgSdms.Master.Federation.DTOs;");
            serviceContent.AppendLine("using NgSdms.Master.Federation.Services.Interfaces;");
            serviceContent.AppendLine("using NgSdms.Master.Management.Entities;");
            serviceContent.AppendLine("using NgSdms.Master.Management.Interfaces;");
            serviceContent.AppendLine();
            serviceContent.AppendLine("namespace NgSdms.Master.Federation.Services.Implementation");
            serviceContent.AppendLine("{");
            serviceContent.AppendLine($"    public class {_entityName}Service : I{_entityName}Service");
            serviceContent.AppendLine("    {");
            serviceContent.AppendLine("        private readonly IMPMRepository _repository;");
            serviceContent.AppendLine("        private readonly IMapper _mapper;");
            serviceContent.AppendLine();
            serviceContent.AppendLine($"        public {_entityName}Service(IMPMRepository repository, IMapper mapper)");
            serviceContent.AppendLine("        {");
            serviceContent.AppendLine("            _repository = repository;");
            serviceContent.AppendLine("            _mapper = mapper;");
            serviceContent.AppendLine("        }");
            serviceContent.AppendLine();
            
            // Implementation of GetByIdAsync
            serviceContent.AppendLine($"        public async Task<{_entityName}Dto?> GetByIdAsync(Guid id)");
            serviceContent.AppendLine("        {");
            serviceContent.AppendLine($"            var {_entityNameLower} = await _repository.UnitOfWork.{_entityName}Repository.GetByIdAsync(id);");
            serviceContent.AppendLine($"            if ({_entityNameLower} == null)");
            serviceContent.AppendLine("                return null;");
            serviceContent.AppendLine("                ");
            serviceContent.AppendLine($"            return _mapper.Map<{_entityName}Dto>({_entityNameLower});");
            serviceContent.AppendLine("        }");
            serviceContent.AppendLine();
            
            // Implementation of GetAllAsync
            serviceContent.AppendLine($"        public async Task<IEnumerable<{_entityName}Dto>> GetAllAsync()");
            serviceContent.AppendLine("        {");
            serviceContent.AppendLine($"            var {_entityNameLower}s = await _repository.UnitOfWork.{_entityName}Repository.GetAllAsync();");
            serviceContent.AppendLine($"            return _mapper.Map<IEnumerable<{_entityName}Dto>>({_entityNameLower}s);");
            serviceContent.AppendLine("        }");
            serviceContent.AppendLine();
            
            // Implementation of CreateAsync
            serviceContent.AppendLine($"        public async Task<{_entityName}Dto> CreateAsync({_entityName}Dto {_entityNameLower}Dto, string createdBy)");
            serviceContent.AppendLine("        {");
            serviceContent.AppendLine($"            var {_entityNameLower} = _mapper.Map<{_entityName}>({_entityNameLower}Dto);");
            serviceContent.AppendLine("            ");
            serviceContent.AppendLine("            // Set audit fields with user information");
            serviceContent.AppendLine($"            {_entityNameLower}.Id = {_entityNameLower}Dto.Id == Guid.Empty ? Guid.NewGuid() : {_entityNameLower}Dto.Id;");
            serviceContent.AppendLine($"            {_entityNameLower}.Createdby = createdBy;");
            serviceContent.AppendLine($"            {_entityNameLower}.Createddate = DateTime.Now;");
            serviceContent.AppendLine("            ");
            serviceContent.AppendLine($"            await _repository.UnitOfWork.{_entityName}Repository.AddAsync({_entityNameLower});");
            serviceContent.AppendLine("            await _repository.UnitOfWork.CompleteAsync();");
            serviceContent.AppendLine($"            return _mapper.Map<{_entityName}Dto>({_entityNameLower});");
            serviceContent.AppendLine("        }");
            serviceContent.AppendLine();
            
            // Implementation of UpdateAsync
            serviceContent.AppendLine($"        public async Task<{_entityName}Dto?> UpdateAsync({_entityName}Dto {_entityNameLower}Dto, string modifiedBy)");
            serviceContent.AppendLine("        {");
            serviceContent.AppendLine($"            var existing{_entityName} = await _repository.UnitOfWork.{_entityName}Repository.GetByIdAsync({_entityNameLower}Dto.Id);");
            serviceContent.AppendLine($"            if (existing{_entityName} == null)");
            serviceContent.AppendLine("                return null;");
            serviceContent.AppendLine();
            serviceContent.AppendLine("            // Map updated values from DTO to entity, preserving entity properties not in DTO");
            serviceContent.AppendLine($"            _mapper.Map({_entityNameLower}Dto, existing{_entityName});");
            serviceContent.AppendLine("            ");
            serviceContent.AppendLine("            // Set audit fields with user information");
            serviceContent.AppendLine($"            existing{_entityName}.Modifiedby = modifiedBy;");
            serviceContent.AppendLine($"            existing{_entityName}.Modifieddate = DateTime.Now;");
            serviceContent.AppendLine();
            serviceContent.AppendLine($"            _repository.UnitOfWork.{_entityName}Repository.Update(existing{_entityName});");
            serviceContent.AppendLine("            await _repository.UnitOfWork.CompleteAsync();");
            serviceContent.AppendLine($"            return _mapper.Map<{_entityName}Dto>(existing{_entityName});");
            serviceContent.AppendLine("        }");
            serviceContent.AppendLine();
            
            // Implementation of DeleteAsync
            serviceContent.AppendLine("        public async Task<bool> DeleteAsync(Guid id, string deletedBy)");
            serviceContent.AppendLine("        {");
            serviceContent.AppendLine($"            var {_entityNameLower} = await _repository.UnitOfWork.{_entityName}Repository.GetByIdAsync(id);");
            serviceContent.AppendLine($"            if ({_entityNameLower} == null)");
            serviceContent.AppendLine("                return false;");
            serviceContent.AppendLine();
            serviceContent.AppendLine("            // Hard delete - actually remove from database");
            serviceContent.AppendLine($"            _repository.UnitOfWork.{_entityName}Repository.Remove({_entityNameLower});");
            serviceContent.AppendLine("            await _repository.UnitOfWork.CompleteAsync();");
            serviceContent.AppendLine("            return true;");
            serviceContent.AppendLine("        }");
            serviceContent.AppendLine();
            
            // Implementation of SoftDeleteAsync
            serviceContent.AppendLine("        public async Task<bool> SoftDeleteAsync(Guid id, string deletedBy)");
            serviceContent.AppendLine("        {");
            serviceContent.AppendLine($"            var {_entityNameLower} = await _repository.UnitOfWork.{_entityName}Repository.GetByIdAsync(id);");
            serviceContent.AppendLine($"            if ({_entityNameLower} == null)");
            serviceContent.AppendLine("                return false;");
            serviceContent.AppendLine();
            serviceContent.AppendLine("            // Soft delete - set deleted fields without removing from database");
            serviceContent.AppendLine($"            {_entityNameLower}.Deletedby = deletedBy;");
            serviceContent.AppendLine($"            {_entityNameLower}.Deleteddate = DateTime.Now;");
            serviceContent.AppendLine();
            serviceContent.AppendLine($"            _repository.UnitOfWork.{_entityName}Repository.Update({_entityNameLower});");
            serviceContent.AppendLine("            await _repository.UnitOfWork.CompleteAsync();");
            serviceContent.AppendLine("            return true;");
            serviceContent.AppendLine("        }");
            serviceContent.AppendLine();
            
            // Implementation of GetByCodeAsync
            serviceContent.AppendLine($"        public async Task<{_entityName}Dto?> GetByCodeAsync(string code)");
            serviceContent.AppendLine("        {");
            serviceContent.AppendLine($"            var {_entityNameLower} = await _repository.UnitOfWork.{_entityName}Repository.GetByCodeAsync(code);");
            serviceContent.AppendLine($"            if ({_entityNameLower} == null)");
            serviceContent.AppendLine("                return null;");
            serviceContent.AppendLine("                ");
            serviceContent.AppendLine($"            return _mapper.Map<{_entityName}Dto>({_entityNameLower});");
            serviceContent.AppendLine("        }");
            
            // Implementation of GetByNameContainingAsync if entity has Name property
            if (_properties.Any(p => p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)))
            {
                serviceContent.AppendLine();
                serviceContent.AppendLine($"        public async Task<IEnumerable<{_entityName}Dto>> GetByNameContainingAsync(string name)");
                serviceContent.AppendLine("        {");
                serviceContent.AppendLine($"            var {_entityNameLower}s = await _repository.UnitOfWork.{_entityName}Repository.GetByNameAsync(name);");
                serviceContent.AppendLine($"            return _mapper.Map<IEnumerable<{_entityName}Dto>>({_entityNameLower}s);");
                serviceContent.AppendLine("        }");
            }
            
            serviceContent.AppendLine("    }");
            serviceContent.AppendLine("}");
            
            File.WriteAllText(Path.Combine(_basePath, "NgSdms.Master.Federation", "Services", "Implementation", $"{_entityName}Service.cs"), serviceContent.ToString());
            Console.WriteLine($"Generated Service: {_entityName}Service.cs");
        }

        private void GenerateController()
        {
            var pluralName = GetPluralName(_entityName);
            
            var controllerContent = new StringBuilder();
            controllerContent.AppendLine("using System;");
            controllerContent.AppendLine("using System.Collections.Generic;");
            controllerContent.AppendLine("using System.Threading.Tasks;");
            controllerContent.AppendLine("using System.Linq;");
            controllerContent.AppendLine("using Microsoft.AspNetCore.Authorization;");
            controllerContent.AppendLine("using Microsoft.AspNetCore.Http;");
            controllerContent.AppendLine("using Microsoft.AspNetCore.Mvc;");
            controllerContent.AppendLine("using NgSdms.Master.Federation.DTOs;");
            controllerContent.AppendLine("using NgSdms.Master.Federation.Services.Interfaces;");
            controllerContent.AppendLine("using NgSdms.Master.Api.Auth;");
            controllerContent.AppendLine("using NgSdms.Master.Infrastructure.Common;");
            controllerContent.AppendLine();
            controllerContent.AppendLine("namespace NgSdms.Master.Api.Controllers");
            controllerContent.AppendLine("{");
            controllerContent.AppendLine("    [Route(\"api/[controller]\")]");
            controllerContent.AppendLine("    [ApiController]");
            controllerContent.AppendLine($"    public class {pluralName}Controller : ControllerBase");
            controllerContent.AppendLine("    {");
            controllerContent.AppendLine("        private readonly IMPMService _service;");
            controllerContent.AppendLine();
            controllerContent.AppendLine($"        public {pluralName}Controller(IMPMService service)");
            controllerContent.AppendLine("        {");
            controllerContent.AppendLine("            _service = service;");
            controllerContent.AppendLine("        }");
            controllerContent.AppendLine();
            
            // GET: api/[PluralName]
            controllerContent.AppendLine($"        // GET: api/{pluralName}");
            controllerContent.AppendLine("        [HttpGet]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status200OK)]");
            
            if (_properties.Any(p => p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)))
            {
                controllerContent.AppendLine($"        public async Task<ActionResult<ApiResponse<IEnumerable<{_entityName}Dto>>>> GetAll{pluralName}(string? name = null)");
                controllerContent.AppendLine("        {");
                controllerContent.AppendLine("            var traceId = HttpContext.GetOrCreateTraceId();");
                controllerContent.AppendLine($"            IEnumerable<{_entityName}Dto> {_entityNameLower}s;");
                controllerContent.AppendLine($"            ApiResponse<IEnumerable<{_entityName}Dto>> response;");
                controllerContent.AppendLine("            if (!string.IsNullOrEmpty(name))");
                controllerContent.AppendLine("            {");
                controllerContent.AppendLine($"                {_entityNameLower}s = await _service.{_entityName}Service.GetByNameContainingAsync(name);");
                controllerContent.AppendLine($"                response = ApiResponse<IEnumerable<{_entityName}Dto>>.Success({_entityNameLower}s, traceId: traceId);");
                controllerContent.AppendLine("                return Ok(response);");
                controllerContent.AppendLine("            }");
                controllerContent.AppendLine();
                controllerContent.AppendLine($"            {_entityNameLower}s = await _service.{_entityName}Service.GetAllAsync();");
                controllerContent.AppendLine($"            response = ApiResponse<IEnumerable<{_entityName}Dto>>.Success({_entityNameLower}s, traceId: traceId);");
                controllerContent.AppendLine("            return Ok(response);");
                controllerContent.AppendLine("        }");
            }
            else
            {
                controllerContent.AppendLine($"        public async Task<ActionResult<ApiResponse<IEnumerable<{_entityName}Dto>>>> GetAll{pluralName}()");
                controllerContent.AppendLine("        {");
                controllerContent.AppendLine("            var traceId = HttpContext.GetOrCreateTraceId();");
                controllerContent.AppendLine($"            var {_entityNameLower}s = await _service.{_entityName}Service.GetAllAsync();");
                controllerContent.AppendLine($"            var response = ApiResponse<IEnumerable<{_entityName}Dto>>.Success({_entityNameLower}s, traceId: traceId);");
                controllerContent.AppendLine("            return Ok(response);");
                controllerContent.AppendLine("        }");
            }
            
            controllerContent.AppendLine();
            
            // GET: api/[PluralName]/5
            controllerContent.AppendLine($"        // GET: api/{pluralName}/5");
            controllerContent.AppendLine("        [HttpGet(\"{id}\")]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status200OK)]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status404NotFound)]");
            controllerContent.AppendLine($"        public async Task<ActionResult<ApiResponse<{_entityName}Dto>>> Get{_entityName}ById(Guid id)");
            controllerContent.AppendLine("        {");
            controllerContent.AppendLine("            var traceId = HttpContext.GetOrCreateTraceId();");
            controllerContent.AppendLine($"            var {_entityNameLower} = await _service.{_entityName}Service.GetByIdAsync(id);");
            controllerContent.AppendLine($"            if ({_entityNameLower} == null)");
            controllerContent.AppendLine($"                return NotFound(ApiResponse<{_entityName}Dto>.Fail($\"{_entityName} with ID {{id}} not found.\", traceId: traceId));");
            controllerContent.AppendLine();
            controllerContent.AppendLine($"            return Ok(ApiResponse<{_entityName}Dto>.Success({_entityNameLower}, traceId: traceId));");
            controllerContent.AppendLine("        }");
            controllerContent.AppendLine();
            
            // GET: api/[PluralName]/code/ABC123
            controllerContent.AppendLine($"        // GET: api/{pluralName}/code/ABC123");
            controllerContent.AppendLine("        [HttpGet(\"code/{code}\")]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status200OK)]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status404NotFound)]");
            controllerContent.AppendLine($"        public async Task<ActionResult<ApiResponse<{_entityName}Dto>>> Get{_entityName}ByCode(string code)");
            controllerContent.AppendLine("        {");
            controllerContent.AppendLine("            var traceId = HttpContext.GetOrCreateTraceId();");
            controllerContent.AppendLine($"            var {_entityNameLower} = await _service.{_entityName}Service.GetByCodeAsync(code);");
            controllerContent.AppendLine($"            if ({_entityNameLower} == null)");
            controllerContent.AppendLine($"                return NotFound(ApiResponse<{_entityName}Dto>.Fail($\"{_entityName} with code {{code}} not found.\", traceId: traceId));");
            controllerContent.AppendLine();
            controllerContent.AppendLine($"            return Ok(ApiResponse<{_entityName}Dto>.Success({_entityNameLower}, traceId: traceId));");
            controllerContent.AppendLine("        }");
            controllerContent.AppendLine();
            
            // POST: api/[PluralName]
            controllerContent.AppendLine($"        // POST: api/{pluralName}");
            controllerContent.AppendLine("        [HttpPost]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status201Created)]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status400BadRequest)]");
            controllerContent.AppendLine($"        public async Task<ActionResult<ApiResponse<{_entityName}Dto>>> Create{_entityName}({_entityName}Dto {_entityNameLower}Dto)");
            controllerContent.AppendLine("        {");
            controllerContent.AppendLine("            var traceId = HttpContext.GetOrCreateTraceId();");
            controllerContent.AppendLine($"            if ({_entityNameLower}Dto == null)");
            controllerContent.AppendLine($"                return BadRequest(ApiResponse<{_entityName}Dto>.Fail(\"{_entityName} data is required.\", traceId: traceId));");
            controllerContent.AppendLine();
            controllerContent.AppendLine("            // Get user info from token for audit fields");
            controllerContent.AppendLine("            var userId = HttpContext.GetUsername() ?? \"system\";");
            controllerContent.AppendLine();
            controllerContent.AppendLine($"            var created{_entityName} = await _service.{_entityName}Service.CreateAsync({_entityNameLower}Dto, userId);");
            controllerContent.AppendLine($"            if (created{_entityName} == null)");
            controllerContent.AppendLine($"                return BadRequest(ApiResponse<{_entityName}Dto>.Fail(\"Failed to create {_entityNameLower}.\", traceId: traceId));");
            controllerContent.AppendLine();
            controllerContent.AppendLine($"            return CreatedAtAction(nameof(Get{_entityName}ById), new {{ id = created{_entityName}.Id }}, ApiResponse<{_entityName}Dto>.Success(created{_entityName}, traceId: traceId));");
            controllerContent.AppendLine("        }");
            controllerContent.AppendLine();
            
            // PUT: api/[PluralName]/5
            controllerContent.AppendLine($"        // PUT: api/{pluralName}/5");
            controllerContent.AppendLine("        [HttpPut(\"{id}\")]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status200OK)]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status404NotFound)]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status400BadRequest)]");
            controllerContent.AppendLine($"        public async Task<ActionResult<ApiResponse<{_entityName}Dto>>> Update{_entityName}(Guid id, {_entityName}Dto {_entityNameLower}Dto)");
            controllerContent.AppendLine("        {");
            controllerContent.AppendLine("            var traceId = HttpContext.GetOrCreateTraceId();");
            controllerContent.AppendLine($"            if ({_entityNameLower}Dto == null)");
            controllerContent.AppendLine($"                return BadRequest(ApiResponse<{_entityName}Dto>.Fail(\"{_entityName} data is required.\", traceId: traceId));");
            controllerContent.AppendLine("            // Ensure DTO Id matches route parameter");
            controllerContent.AppendLine($"            {_entityNameLower}Dto.Id = id;");
            controllerContent.AppendLine();
            controllerContent.AppendLine("            // Get user info from token for audit fields");
            controllerContent.AppendLine("            var userId = HttpContext.GetUsername() ?? \"system\";");
            controllerContent.AppendLine();
            controllerContent.AppendLine($"            var updated{_entityName} = await _service.{_entityName}Service.UpdateAsync({_entityNameLower}Dto, userId);");
            controllerContent.AppendLine($"            if (updated{_entityName} == null)");
            controllerContent.AppendLine($"                return NotFound(ApiResponse<{_entityName}Dto>.Fail($\"{_entityName} with ID {{id}} not found.\", traceId: traceId));");
            controllerContent.AppendLine();
            controllerContent.AppendLine($"            return Ok(ApiResponse<{_entityName}Dto>.Success(updated{_entityName}, traceId: traceId));");
            controllerContent.AppendLine("        }");
            controllerContent.AppendLine();
            
            // DELETE: api/[PluralName]/5
            controllerContent.AppendLine($"        // DELETE: api/{pluralName}/5 (Soft Delete)");
            controllerContent.AppendLine("        [HttpDelete(\"{id}\")]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status204NoContent)]");
            controllerContent.AppendLine("        [ProducesResponseType(StatusCodes.Status404NotFound)]");
            controllerContent.AppendLine($"        public async Task<ActionResult<ApiResponse<object>>> Delete{_entityName}(Guid id)");
            controllerContent.AppendLine("        {");
            controllerContent.AppendLine("            var traceId = HttpContext.GetOrCreateTraceId();");
            controllerContent.AppendLine("            // Get user info from token for audit fields");
            controllerContent.AppendLine("            var userId = HttpContext.GetUsername() ?? \"system\";");
            controllerContent.AppendLine();
            controllerContent.AppendLine($"            var result = await _service.{_entityName}Service.SoftDeleteAsync(id, userId);");
            controllerContent.AppendLine("            if (!result)");
            controllerContent.AppendLine($"                return NotFound(ApiResponse<object>.Fail($\"{_entityName} with ID {{id}} not found.\", traceId: traceId));");
            controllerContent.AppendLine();
            controllerContent.AppendLine($"            return Ok(ApiResponse<object>.Success(null, \"{_entityName} deleted successfully.\", traceId: traceId));");
            controllerContent.AppendLine("        }");
            controllerContent.AppendLine("    }");
            controllerContent.AppendLine("}");
            
            File.WriteAllText(Path.Combine(_basePath, "NgSdms.Master.API", "Controllers", $"{pluralName}Controller.cs"), controllerContent.ToString());
            Console.WriteLine($"Generated Controller: {pluralName}Controller.cs");
        }

        private string GetPluralName(string name)
        {
            // Simple pluralization logic - can be improved
            if (name.EndsWith("y"))
                return name.Substring(0, name.Length - 1) + "ies";
            if (name.EndsWith("s") || name.EndsWith("x") || name.EndsWith("z") || name.EndsWith("ch") || name.EndsWith("sh"))
                return name + "es";
            return name + "s";
        }
    }

    public class PropertyDefinition
    {
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public string Type { get; set; }
        public bool IsNullable { get; set; }
        public int MaxLength { get; set; }

        public PropertyDefinition(string name, string columnName, string type, bool isNullable = true, int maxLength = 0)
        {
            Name = name;
            ColumnName = columnName;
            Type = type;
            IsNullable = isNullable;
            MaxLength = maxLength;
        }
    }
}

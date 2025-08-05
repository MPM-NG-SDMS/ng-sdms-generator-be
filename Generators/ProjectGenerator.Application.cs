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
            // Scaffold sample Product DTO, service interface, service implementation for CRUD in Federation project.
            // Scaffold Product DTO
            var dtosDir = Path.Combine(projectDir, "DTOs");
            var productDto = new StringBuilder();
            productDto.AppendLine("using System;");
            productDto.AppendLine();
            productDto.AppendLine($"namespace {_projectName}.Federation.DTOs");
            productDto.AppendLine("{");
            productDto.AppendLine("    public class ProductDto : BaseDto");
            productDto.AppendLine("    {");
            productDto.AppendLine("        public string Name { get; set; } = string.Empty;");
            productDto.AppendLine("        public decimal Price { get; set; }");
            productDto.AppendLine("    }");
            productDto.AppendLine("}");
            File.WriteAllText(Path.Combine(dtosDir, "ProductDto.cs"), productDto.ToString());

            // Scaffold DTO mapping for Product
            var mappingsDir = Path.Combine(projectDir, "Mappings");
            var mappingProfilePath = Path.Combine(mappingsDir, $"{_projectName}MappingProfile.cs");
            // Insert mapping code into existing profile
            var profileContent = File.ReadAllText(mappingProfilePath);
            var insertIndex = profileContent.IndexOf("}", profileContent.LastIndexOf("public class"));
            var mappingCode = "\n            CreateMap<Management.Entities.Product, Federation.DTOs.ProductDto>();\n            CreateMap<Federation.DTOs.ProductDto, Management.Entities.Product>();\n";
            profileContent = profileContent.Insert(insertIndex, mappingCode);
            File.WriteAllText(mappingProfilePath, profileContent);

            // Scaffold IProductService interface
            var servicesInterfacesDir = Path.Combine(projectDir, "Services", "Interfaces");
            var productServiceInterface = new StringBuilder();
            productServiceInterface.AppendLine("using System;");
            productServiceInterface.AppendLine("using System.Collections.Generic;");
            productServiceInterface.AppendLine("using System.Threading.Tasks;");
            productServiceInterface.AppendLine();
            productServiceInterface.AppendLine($"namespace {_projectName}.Federation.Services.Interfaces");
            productServiceInterface.AppendLine("{");
            productServiceInterface.AppendLine("    public interface IProductService");
            productServiceInterface.AppendLine("    {");
            productServiceInterface.AppendLine("        Task<IEnumerable<ProductDto>> GetAllAsync();");
            productServiceInterface.AppendLine("        Task<ProductDto?> GetByIdAsync(Guid id);");
            productServiceInterface.AppendLine("        Task<ProductDto> CreateAsync(ProductDto dto);");
            productServiceInterface.AppendLine("        Task UpdateAsync(ProductDto dto);");
            productServiceInterface.AppendLine("        Task DeleteAsync(Guid id);");
            productServiceInterface.AppendLine("    }");
            productServiceInterface.AppendLine("}");
            File.WriteAllText(Path.Combine(servicesInterfacesDir, "IProductService.cs"), productServiceInterface.ToString());

            // Scaffold ProductService implementation
            var servicesImplDir = Path.Combine(projectDir, "Services", "Implementation");
            var productServiceImpl = new StringBuilder();
            productServiceImpl.AppendLine("using System;");
            productServiceImpl.AppendLine("using System.Collections.Generic;");
            productServiceImpl.AppendLine("using System.Threading.Tasks;");
            productServiceImpl.AppendLine("using AutoMapper;");
            productServiceImpl.AppendLine($"using {_projectName}.Federation.Services.Interfaces;");
            productServiceImpl.AppendLine($"using {_projectName}.Federation.DTOs;");
            productServiceImpl.AppendLine($"using {_projectName}.Management.Repositories.Interfaces;");
            productServiceImpl.AppendLine();
            productServiceImpl.AppendLine($"namespace {_projectName}.Federation.Services.Implementation");
            productServiceImpl.AppendLine("{");
            productServiceImpl.AppendLine("    public class ProductService : IProductService");
            productServiceImpl.AppendLine("    {");
            productServiceImpl.AppendLine("        private readonly IProductRepository _repository;");
            productServiceImpl.AppendLine("        private readonly IMapper _mapper;");
            productServiceImpl.AppendLine();
            productServiceImpl.AppendLine("        public ProductService(IProductRepository repository, IMapper mapper)");
            productServiceImpl.AppendLine("        {");
            productServiceImpl.AppendLine("            _repository = repository;");
            productServiceImpl.AppendLine("            _mapper = mapper;");
            productServiceImpl.AppendLine("        }");
            productServiceImpl.AppendLine();
            productServiceImpl.AppendLine("        public async Task<IEnumerable<ProductDto>> GetAllAsync() =>");
            productServiceImpl.AppendLine("            _mapper.Map<IEnumerable<ProductDto>>(await _repository.GetAllAsync());");
            productServiceImpl.AppendLine();
            productServiceImpl.AppendLine("        public async Task<ProductDto?> GetByIdAsync(Guid id) =>");
            productServiceImpl.AppendLine("            _mapper.Map<ProductDto?>(await _repository.GetByIdAsync(id));");
            productServiceImpl.AppendLine();
            productServiceImpl.AppendLine("        public async Task<ProductDto> CreateAsync(ProductDto dto)");
            productServiceImpl.AppendLine("        {");
            productServiceImpl.AppendLine("            var entity = _mapper.Map<Management.Entities.Product>(dto);");
            productServiceImpl.AppendLine("            await _repository.AddAsync(entity);");
            productServiceImpl.AppendLine("            await _repository.UnitOfWork.CompleteAsync();");
            productServiceImpl.AppendLine("            return _mapper.Map<ProductDto>(entity);");
            productServiceImpl.AppendLine("        }");
            productServiceImpl.AppendLine();
            productServiceImpl.AppendLine("        public async Task UpdateAsync(ProductDto dto)");
            productServiceImpl.AppendLine("        {");
            productServiceImpl.AppendLine("            var entity = _mapper.Map<Management.Entities.Product>(dto);");
            productServiceImpl.AppendLine("            _repository.Update(entity);");
            productServiceImpl.AppendLine("            await _repository.UnitOfWork.CompleteAsync();");
            productServiceImpl.AppendLine("        }");
            productServiceImpl.AppendLine();
            productServiceImpl.AppendLine("        public async Task DeleteAsync(Guid id)");
            productServiceImpl.AppendLine("        {");
            productServiceImpl.AppendLine("            var entity = await _repository.GetByIdAsync(id);");
            productServiceImpl.AppendLine("            if (entity == null) return;");
            productServiceImpl.AppendLine("            _repository.Remove(_mapper.Map<Management.Entities.Product>(entity));");
            productServiceImpl.AppendLine("            await _repository.UnitOfWork.CompleteAsync();");
            productServiceImpl.AppendLine("        }");
            productServiceImpl.AppendLine("    }");
            productServiceImpl.AppendLine("}");
            File.WriteAllText(Path.Combine(servicesImplDir, "ProductService.cs"), productServiceImpl.ToString());
        }
    }
}

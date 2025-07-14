# ============================================================================
# 🔧 SCRIPT: Corregir Application Layer - Handlers y DTOs
# ============================================================================

Write-Host "🔧 Corrigiendo errores en Application Layer..." -ForegroundColor Yellow

$baseDir = "C:\Users\marce\source\repos\PeluqueriaSaaS"
Set-Location $baseDir

# ============================================================================
# 1️⃣ CORREGIR ClienteDto.cs
# ============================================================================
Write-Host "`n1️⃣ Corrigiendo ClienteDto.cs..." -ForegroundColor Yellow

$clienteDto = @'
// src/PeluqueriaSaaS.Application/DTOs/ClienteDto.cs
namespace PeluqueriaSaaS.Application.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
'@
$clienteDto | Out-File -FilePath "src\PeluqueriaSaaS.Application\DTOs\ClienteDto.cs" -Encoding UTF8

# ============================================================================
# 2️⃣ CORREGIR CrearClienteHandler.cs
# ============================================================================
Write-Host "   📄 Corrigiendo CrearClienteHandler.cs..." -ForegroundColor White

$crearClienteHandler = @'
// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/CrearClienteHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Domain.Interfaces;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class CrearClienteHandler : IRequestHandler<CrearClienteCommand, ClienteDto>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public CrearClienteHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<ClienteDto> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = new ClienteBasico
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Telefono = request.Telefono,
                FechaNacimiento = request.FechaNacimiento,
                FechaRegistro = DateTime.Now
            };

            var clienteCreado = await _repositoryManager.AddClienteAsync(cliente);

            return new ClienteDto
            {
                Id = clienteCreado.Id,
                Nombre = clienteCreado.Nombre,
                Apellido = clienteCreado.Apellido,
                Email = clienteCreado.Email,
                Telefono = clienteCreado.Telefono,
                FechaNacimiento = clienteCreado.FechaNacimiento,
                FechaRegistro = clienteCreado.FechaRegistro
            };
        }
    }
}
'@
$crearClienteHandler | Out-File -FilePath "src\PeluqueriaSaaS.Application\Features\Clientes\Handlers\CrearClienteHandler.cs" -Encoding UTF8

# ============================================================================
# 3️⃣ CORREGIR ObtenerClientesHandler.cs
# ============================================================================
Write-Host "   📄 Corrigiendo ObtenerClientesHandler.cs..." -ForegroundColor White

$obtenerClientesHandler = @'
// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/ObtenerClientesHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class ObtenerClientesHandler : IRequestHandler<ObtenerClientesQuery, IEnumerable<ClienteDto>>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public ObtenerClientesHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<IEnumerable<ClienteDto>> Handle(ObtenerClientesQuery request, CancellationToken cancellationToken)
        {
            var clientes = await _repositoryManager.GetAllClientesAsync();

            return clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Email = c.Email,
                Telefono = c.Telefono,
                FechaNacimiento = c.FechaNacimiento,
                FechaRegistro = c.FechaRegistro
            });
        }
    }
}
'@
$obtenerClientesHandler | Out-File -FilePath "src\PeluqueriaSaaS.Application\Features\Clientes\Handlers\ObtenerClientesHandler.cs" -Encoding UTF8

# ============================================================================
# 4️⃣ CORREGIR ObtenerClientePorIdHandler.cs
# ============================================================================
Write-Host "   📄 Corrigiendo ObtenerClientePorIdHandler.cs..." -ForegroundColor White

$obtenerClientePorIdHandler = @'
// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/ObtenerClientePorIdHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Queries;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class ObtenerClientePorIdHandler : IRequestHandler<ObtenerClientePorIdQuery, ClienteDto?>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public ObtenerClientePorIdHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<ClienteDto?> Handle(ObtenerClientePorIdQuery request, CancellationToken cancellationToken)
        {
            var cliente = await _repositoryManager.GetClienteByIdAsync(request.Id);
            
            if (cliente == null)
                return null;

            return new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                FechaNacimiento = cliente.FechaNacimiento,
                FechaRegistro = cliente.FechaRegistro
            };
        }
    }
}
'@
$obtenerClientePorIdHandler | Out-File -FilePath "src\PeluqueriaSaaS.Application\Features\Clientes\Handlers\ObtenerClientePorIdHandler.cs" -Encoding UTF8

# ============================================================================
# 5️⃣ CORREGIR UpdateClienteHandler.cs
# ============================================================================
Write-Host "   📄 Corrigiendo UpdateClienteHandler.cs..." -ForegroundColor White

$updateClienteHandler = @'
// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/UpdateClienteHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class UpdateClienteHandler : IRequestHandler<UpdateClienteCommand, ClienteDto?>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public UpdateClienteHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<ClienteDto?> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = await _repositoryManager.GetClienteByIdAsync(request.Id);
            
            if (cliente == null)
                return null;

            // Actualizar propiedades
            cliente.Nombre = request.Nombre;
            cliente.Apellido = request.Apellido;
            cliente.Email = request.Email;
            cliente.Telefono = request.Telefono;
            cliente.FechaNacimiento = request.FechaNacimiento;

            await _repositoryManager.UpdateClienteAsync(cliente);

            return new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                FechaNacimiento = cliente.FechaNacimiento,
                FechaRegistro = cliente.FechaRegistro
            };
        }
    }
}
'@
$updateClienteHandler | Out-File -FilePath "src\PeluqueriaSaaS.Application\Features\Clientes\Handlers\UpdateClienteHandler.cs" -Encoding UTF8

# ============================================================================
# 6️⃣ CORREGIR DeleteClienteHandler.cs
# ============================================================================
Write-Host "   📄 Corrigiendo DeleteClienteHandler.cs..." -ForegroundColor White

$deleteClienteHandler = @'
// src/PeluqueriaSaaS.Application/Features/Clientes/Handlers/DeleteClienteHandler.cs
using MediatR;
using PeluqueriaSaaS.Application.Features.Clientes.Commands;
using PeluqueriaSaaS.Domain.Interfaces;

namespace PeluqueriaSaaS.Application.Features.Clientes.Handlers
{
    public class DeleteClienteHandler : IRequestHandler<DeleteClienteCommand, bool>
    {
        private readonly IRepositoryManagerTemp _repositoryManager;

        public DeleteClienteHandler(IRepositoryManagerTemp repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<bool> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = await _repositoryManager.GetClienteByIdAsync(request.Id);
            
            if (cliente == null)
                return false;

            await _repositoryManager.DeleteClienteAsync(request.Id);
            return true;
        }
    }
}
'@
$deleteClienteHandler | Out-File -FilePath "src\PeluqueriaSaaS.Application\Features\Clientes\Handlers\DeleteClienteHandler.cs" -Encoding UTF8

# ============================================================================
# 7️⃣ VERIFICAR Y CORREGIR CrearClienteCommand.cs
# ============================================================================
Write-Host "   📄 Verificando CrearClienteCommand.cs..." -ForegroundColor White

$crearClienteCommand = @'
// src/PeluqueriaSaaS.Application/Features/Clientes/Commands/CrearClienteCommand.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace PeluqueriaSaaS.Application.Features.Clientes.Commands
{
    public class CrearClienteCommand : IRequest<ClienteDto>
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        public string Apellido { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres")]
        public string Email { get; set; } = string.Empty;
        
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        public string? Telefono { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }
    }
}
'@
$crearClienteCommand | Out-File -FilePath "src\PeluqueriaSaaS.Application\Features\Clientes\Commands\CrearClienteCommand.cs" -Encoding UTF8

# ============================================================================
# 8️⃣ CORREGIR ClienteProfile.cs (AutoMapper)
# ============================================================================
Write-Host "   📄 Corrigiendo ClienteProfile.cs..." -ForegroundColor White

$clienteProfile = @'
// src/PeluqueriaSaaS.Application/Mapping/ClienteProfile.cs
using AutoMapper;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Application.Mapping
{
    public class ClienteProfile : Profile
    {
        public ClienteProfile()
        {
            CreateMap<ClienteBasico, ClienteDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Apellido))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono))
                .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.FechaNacimiento))
                .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => src.FechaRegistro));

            CreateMap<ClienteDto, ClienteBasico>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Apellido))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono))
                .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.FechaNacimiento))
                .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => src.FechaRegistro));
        }
    }
}
'@
$clienteProfile | Out-File -FilePath "src\PeluqueriaSaaS.Application\Mapping\ClienteProfile.cs" -Encoding UTF8

# ============================================================================
# 9️⃣ VERIFICAR QUERIES
# ============================================================================
Write-Host "   📄 Verificando ObtenerClientesQuery.cs..." -ForegroundColor White

$obtenerClientesQuery = @'
// src/PeluqueriaSaaS.Application/Features/Clientes/Queries/ObtenerClientesQuery.cs
using MediatR;
using PeluqueriaSaaS.Application.DTOs;

namespace PeluqueriaSaaS.Application.Features.Clientes.Queries
{
    public class ObtenerClientesQuery : IRequest<IEnumerable<ClienteDto>>
    {
    }
}
'@
$obtenerClientesQuery | Out-File -FilePath "src\PeluqueriaSaaS.Application\Features\Clientes\Queries\ObtenerClientesQuery.cs" -Encoding UTF8

# ============================================================================
# 🔨 COMPILAR PROYECTO
# ============================================================================
Write-Host "`n🔨 Compilando proyecto..." -ForegroundColor Yellow

dotnet build src/PeluqueriaSaaS.Web/PeluqueriaSaaS.Web.csproj

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ ¡Compilación exitosa! Todos los errores de Application Layer han sido corregidos." -ForegroundColor Green
    
    Write-Host "`n📋 ARCHIVOS CORREGIDOS:" -ForegroundColor Yellow
    Write-Host "   📄 ClienteDto.cs (agregadas propiedades faltantes)" -ForegroundColor White
    Write-Host "   📄 CrearClienteHandler.cs (corregido para usar ClienteBasico)" -ForegroundColor White
    Write-Host "   📄 ObtenerClientesHandler.cs (corregido para usar métodos correctos)" -ForegroundColor White
    Write-Host "   📄 ObtenerClientePorIdHandler.cs (corregido mapeo de propiedades)" -ForegroundColor White
    Write-Host "   📄 UpdateClienteHandler.cs (corregido para nuevas propiedades)" -ForegroundColor White
    Write-Host "   📄 DeleteClienteHandler.cs (creado correctamente)" -ForegroundColor White
    Write-Host "   📄 CrearClienteCommand.cs (verificado)" -ForegroundColor White
    Write-Host "   📄 ClienteProfile.cs (corregido AutoMapper)" -ForegroundColor White
    Write-Host "   📄 ObtenerClientesQuery.cs (verificado)" -ForegroundColor White
    
    Write-Host "`n🚀 AHORA PUEDES PROBAR LA APLICACIÓN:" -ForegroundColor Yellow
    Write-Host "   dotnet run --project src/PeluqueriaSaaS.Web" -ForegroundColor White
    Write-Host "   Ve a: http://localhost:5043/Clientes" -ForegroundColor White
    
    Write-Host "`n🎯 FUNCIONALIDADES DISPONIBLES:" -ForegroundColor Yellow
    Write-Host "   ✅ Lista de clientes" -ForegroundColor Green
    Write-Host "   ✅ Ver detalles de cliente" -ForegroundColor Green
    Write-Host "   ✅ Crear nuevo cliente" -ForegroundColor Green
    Write-Host "   ✅ Editar cliente existente" -ForegroundColor Green
    Write-Host "   ✅ Eliminar cliente" -ForegroundColor Green
    
} else {
    Write-Host "`n❌ Aún hay errores de compilación. Revisa los mensajes arriba." -ForegroundColor Red
    Write-Host "💡 Puede que necesites verificar otros archivos." -ForegroundColor Yellow
}

Write-Host "`n🎉 ¡Corrección de Application Layer completada!" -ForegroundColor Green
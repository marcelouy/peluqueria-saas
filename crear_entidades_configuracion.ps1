# Script para crear entidades de configuración - Peluquería SaaS
# Resolver errores de compilación urgentemente

$basePath = "C:\Users\marce\source\repos\PeluqueriaSaaS\src\PeluqueriaSaaS.Domain\Entities"
$configPath = Join-Path $basePath "Configuration"

# Crear carpeta Configuration si no existe
if (-Not (Test-Path $configPath)) {
    Write-Host "Creando carpeta Configuration..." -ForegroundColor Green
    New-Item -ItemType Directory -Path $configPath -Force
}

# 1. ConfiguracionBase.cs
$configuracionBase = @'
using PeluqueriaSaaS.Domain.Entities.Base;

namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public abstract class ConfiguracionBase : TenantEntityBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nombre { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; }
        public string? Color { get; private set; }
        public string? Icono { get; private set; }
        public int Orden { get; private set; }
        public bool EsActivo { get; private set; } = true;
        public bool EsPorDefecto { get; private set; } = false;
        public bool EsSistema { get; private set; } = false;

        protected ConfiguracionBase() { }

        protected ConfiguracionBase(string codigo, string nombre, string? descripcion = null, string? color = null, string? icono = null, int orden = 0, bool esPorDefecto = false, bool esSistema = false)
        {
            Codigo = codigo;
            Nombre = nombre;
            Descripcion = descripcion;
            Color = color;
            Icono = icono;
            Orden = orden;
            EsPorDefecto = esPorDefecto;
            EsSistema = esSistema;
        }

        public void ActualizarInformacion(string nombre, string? descripcion = null, string? color = null, string? icono = null, int orden = 0)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Color = color;
            Icono = icono;
            Orden = orden;
        }

        public void Activar() => EsActivo = true;
        public void Desactivar() => EsActivo = false;
        public void MarcarComoPorDefecto() => EsPorDefecto = true;
        public void QuitarPorDefecto() => EsPorDefecto = false;
    }
}
'@

# 2. TipoServicio.cs
$tipoServicio = @'
namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public class TipoServicio : ConfiguracionBase
    {
        public int DuracionEstimadaMinutos { get; private set; }
        public decimal PrecioBase { get; private set; }
        public bool RequiereEspecialista { get; private set; } = false;
        public bool PermiteDescuentos { get; private set; } = true;
        public int? TiempoPreparacionMinutos { get; private set; }

        private TipoServicio() { }

        public TipoServicio(string codigo, string nombre, int duracionEstimadaMinutos, decimal precioBase, string? descripcion = null, bool requiereEspecialista = false, bool permiteDescuentos = true, int? tiempoPreparacionMinutos = null, string? color = null, string? icono = null, int orden = 0, bool esPorDefecto = false, bool esSistema = false)
            : base(codigo, nombre, descripcion, color, icono, orden, esPorDefecto, esSistema)
        {
            DuracionEstimadaMinutos = duracionEstimadaMinutos;
            PrecioBase = precioBase;
            RequiereEspecialista = requiereEspecialista;
            PermiteDescuentos = permiteDescuentos;
            TiempoPreparacionMinutos = tiempoPreparacionMinutos;
        }

        public void ActualizarConfiguracion(int duracionEstimadaMinutos, decimal precioBase, bool requiereEspecialista = false, bool permiteDescuentos = true, int? tiempoPreparacionMinutos = null)
        {
            DuracionEstimadaMinutos = duracionEstimadaMinutos;
            PrecioBase = precioBase;
            RequiereEspecialista = requiereEspecialista;
            PermiteDescuentos = permiteDescuentos;
            TiempoPreparacionMinutos = tiempoPreparacionMinutos;
        }
    }
}
'@

# 3. EstadoCita.cs
$estadoCita = @'
namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public class EstadoCita : ConfiguracionBase
    {
        public bool EsEstadoInicial { get; private set; } = false;
        public bool EsEstadoFinal { get; private set; } = false;
        public bool PermiteCancelacion { get; private set; } = true;
        public bool PermiteModificacion { get; private set; } = true;
        public bool RequiereConfirmacion { get; private set; } = false;
        public bool NotificarCliente { get; private set; } = true;
        public bool NotificarEmpleado { get; private set; } = true;

        private readonly List<EstadoCita> _estadosPermitidos = new();
        public IReadOnlyList<EstadoCita> EstadosPermitidos => _estadosPermitidos.AsReadOnly();

        private EstadoCita() { }

        public EstadoCita(string codigo, string nombre, string? descripcion = null, bool esEstadoInicial = false, bool esEstadoFinal = false, bool permiteCancelacion = true, bool permiteModificacion = true, bool requiereConfirmacion = false, bool notificarCliente = true, bool notificarEmpleado = true, string? color = null, string? icono = null, int orden = 0, bool esPorDefecto = false, bool esSistema = false)
            : base(codigo, nombre, descripcion, color, icono, orden, esPorDefecto, esSistema)
        {
            EsEstadoInicial = esEstadoInicial;
            EsEstadoFinal = esEstadoFinal;
            PermiteCancelacion = permiteCancelacion;
            PermiteModificacion = permiteModificacion;
            RequiereConfirmacion = requiereConfirmacion;
            NotificarCliente = notificarCliente;
            NotificarEmpleado = notificarEmpleado;
        }

        public void AgregarEstadoPermitido(EstadoCita estado)
        {
            if (!_estadosPermitidos.Contains(estado))
            {
                _estadosPermitidos.Add(estado);
            }
        }

        public bool PuedeTransicionarA(EstadoCita estado)
        {
            return _estadosPermitidos.Contains(estado);
        }
    }
}
'@

# 4. EstadoEmpleado.cs
$estadoEmpleado = @'
namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public class EstadoEmpleado : ConfiguracionBase
    {
        public bool PuedeAtenderClientes { get; private set; } = true;
        public bool PuedeRecibirCitas { get; private set; } = true;
        public bool EsVisible { get; private set; } = true;
        public bool RequiereJustificacion { get; private set; } = false;
        public int? TiempoMaximoMinutos { get; private set; }

        private EstadoEmpleado() { }

        public EstadoEmpleado(string codigo, string nombre, string? descripcion = null, bool puedeAtenderClientes = true, bool puedeRecibirCitas = true, bool esVisible = true, bool requiereJustificacion = false, int? tiempoMaximoMinutos = null, string? color = null, string? icono = null, int orden = 0, bool esPorDefecto = false, bool esSistema = false)
            : base(codigo, nombre, descripcion, color, icono, orden, esPorDefecto, esSistema)
        {
            PuedeAtenderClientes = puedeAtenderClientes;
            PuedeRecibirCitas = puedeRecibirCitas;
            EsVisible = esVisible;
            RequiereJustificacion = requiereJustificacion;
            TiempoMaximoMinutos = tiempoMaximoMinutos;
        }
    }
}
'@

# 5. TipoEstacion.cs
$tipoEstacion = @'
namespace PeluqueriaSaaS.Domain.Entities.Configuration
{
    public class TipoEstacion : ConfiguracionBase
    {
        public int CapacidadMaxima { get; private set; } = 1;
        public bool RequiereEquipoEspecial { get; private set; } = false;
        public string? EquipoRequerido { get; private set; }
        public bool EsMovil { get; private set; } = false;
        public int? TiempoLimpiezaMinutos { get; private set; }

        private readonly List<TipoServicio> _tiposServicioCompatibles = new();
        public IReadOnlyList<TipoServicio> TiposServicioCompatibles => _tiposServicioCompatibles.AsReadOnly();

        private TipoEstacion() { }

        public TipoEstacion(string codigo, string nombre, string? descripcion = null, int capacidadMaxima = 1, bool requiereEquipoEspecial = false, string? equipoRequerido = null, bool esMovil = false, int? tiempoLimpiezaMinutos = null, string? color = null, string? icono = null, int orden = 0, bool esPorDefecto = false, bool esSistema = false)
            : base(codigo, nombre, descripcion, color, icono, orden, esPorDefecto, esSistema)
        {
            CapacidadMaxima = capacidadMaxima;
            RequiereEquipoEspecial = requiereEquipoEspecial;
            EquipoRequerido = equipoRequerido;
            EsMovil = esMovil;
            TiempoLimpiezaMinutos = tiempoLimpiezaMinutos;
        }

        public void AgregarTipoServicioCompatible(TipoServicio tipoServicio)
        {
            if (!_tiposServicioCompatibles.Contains(tipoServicio))
            {
                _tiposServicioCompatibles.Add(tipoServicio);
            }
        }

        public bool EsCompatibleConServicio(TipoServicio tipoServicio)
        {
            return _tiposServicioCompatibles.Contains(tipoServicio);
        }
    }
}
'@

# Crear todos los archivos
Write-Host "Creando archivos de configuración..." -ForegroundColor Yellow

$archivos = @{
    "ConfiguracionBase.cs" = $configuracionBase
    "TipoServicio.cs" = $tipoServicio
    "EstadoCita.cs" = $estadoCita
    "EstadoEmpleado.cs" = $estadoEmpleado
    "TipoEstacion.cs" = $tipoEstacion
}

foreach ($archivo in $archivos.GetEnumerator()) {
    $rutaCompleta = Join-Path $configPath $archivo.Key
    Set-Content -Path $rutaCompleta -Value $archivo.Value -Encoding UTF8
    Write-Host "✅ Creado: $($archivo.Key)" -ForegroundColor Green
}

Write-Host "`n🎯 ARCHIVOS CREADOS EXITOSAMENTE:" -ForegroundColor Cyan
Write-Host "Carpeta: $configPath" -ForegroundColor Gray
Write-Host "- ConfiguracionBase.cs (Clase base abstracta)" -ForegroundColor White
Write-Host "- TipoServicio.cs (Servicios configurables)" -ForegroundColor White
Write-Host "- EstadoCita.cs (Estados con transiciones)" -ForegroundColor White
Write-Host "- EstadoEmpleado.cs (Estados de empleados)" -ForegroundColor White
Write-Host "- TipoEstacion.cs (Tipos de estaciones)" -ForegroundColor White

Write-Host "`n🔧 SIGUIENTE PASO - VERIFICAR COMPILACIÓN:" -ForegroundColor Yellow
Write-Host "cd C:\Users\marce\source\repos\PeluqueriaSaaS" -ForegroundColor Gray
Write-Host "dotnet build" -ForegroundColor Gray

Write-Host "`n📦 DESPUÉS INSTALAR PAQUETES NUGET:" -ForegroundColor Yellow
Write-Host "dotnet add src/PeluqueriaSaaS.Domain package FluentValidation --version 11.8.1" -ForegroundColor Gray
Write-Host "dotnet add src/PeluqueriaSaaS.Application package MediatR --version 12.2.0" -ForegroundColor Gray
Write-Host "dotnet add src/PeluqueriaSaaS.Application package FluentValidation --version 11.8.1" -ForegroundColor Gray
Write-Host "dotnet add src/PeluqueriaSaaS.Application package AutoMapper --version 12.0.1" -ForegroundColor Gray
Write-Host "dotnet add src/PeluqueriaSaaS.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.1" -ForegroundColor Gray
Write-Host "dotnet add src/PeluqueriaSaaS.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 8.0.1" -ForegroundColor Gray
Write-Host "dotnet add src/PeluqueriaSaaS.Web package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.1" -ForegroundColor Gray
Write-Host "dotnet add src/PeluqueriaSaaS.Web package System.IdentityModel.Tokens.Jwt --version 7.1.2" -ForegroundColor Gray
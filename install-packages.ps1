rite-Host "Instalando paquetes NuGet para PeluqueriaSaaS..." -ForegroundColor Green

# Domain Project
Write-Host "Instalando paquetes para Domain..." -ForegroundColor Yellow
Set-Location "src/PeluqueriaSaaS.Domain"
dotnet add package FluentValidation --version 11.8.1

# Application Project  
Write-Host "Instalando paquetes para Application..." -ForegroundColor Yellow
Set-Location "../PeluqueriaSaaS.Application"
dotnet add package MediatR --version 12.2.0
dotnet add package FluentValidation --version 11.8.1
dotnet add package FluentValidation.DependencyInjection --version 11.8.1
dotnet add package AutoMapper --version 12.0.1
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0
dotnet add package Microsoft.Extensions.Logging.Abstractions --version 8.0.0

# Infrastructure Project
Write-Host "Instalando paquetes para Infrastructure..." -ForegroundColor Yellow
Set-Location "../PeluqueriaSaaS.Infrastructure"
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.1
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.1
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.1
dotnet add package Serilog.AspNetCore --version 8.0.0
dotnet add package Serilog.Sinks.SqlServer --version 6.0.0
dotnet add package Microsoft.EntityFrameworkCore.Abstractions --version 8.0.1

# Web Project
Write-Host "Instalando paquetes para Web..." -ForegroundColor Yellow
Set-Location "../PeluqueriaSaaS.Web"
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.1
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.0.3
dotnet add package Blazorise --version 1.4.2
dotnet add package Blazorise.Bootstrap5 --version 1.4.2
dotnet add package Blazorise.Icons.FontAwesome --version 1.4.2
dotnet add package Microsoft.AspNetCore.Http.Features --version 5.0.17

# Shared Project
Write-Host "Instalando paquetes para Shared..." -ForegroundColor Yellow
Set-Location "../PeluqueriaSaaS.Shared"
dotnet add package System.ComponentModel.Annotations --version 5.0.0

# Volver a la raíz
Set-Location "../../"

Write-Host "¡Instalación completada!" -ForegroundColor Green
Write-Host "Ejecuta 'dotnet restore' para restaurar las dependencias." -ForegroundColor Cyan
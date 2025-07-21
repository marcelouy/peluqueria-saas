# 🚨 DIAGNÓSTICO EMERGENCY - RECOVERY MIGRACIÓN POS
# Fecha: Julio 21, 2025
# Objetivo: Determinar estado actual sistema después de error PS
# EJECUTAR LÍNEA POR LÍNEA - NO como script completo

Write-Host "🚨 INICIANDO DIAGNÓSTICO EMERGENCY..." -ForegroundColor Red
Write-Host ""

Write-Host "🔍 PASO 1: Verificando build del proyecto..." -ForegroundColor Yellow
try {
    dotnet build .\src\PeluqueriaSaaS.Web
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ BUILD: OK - Proyecto compila sin errores" -ForegroundColor Green
    } else {
        Write-Host "❌ BUILD: ERROR - Hay errores de compilación" -ForegroundColor Red
        Write-Host "   🚨 POSIBLE CAUSA: DbContext actualizado incorrectamente" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ BUILD: EXCEPCIÓN - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🔍 PASO 2: Verificando estado migraciones EF..." -ForegroundColor Yellow
try {
    Write-Host "📋 Listando migraciones aplicadas..." -ForegroundColor Cyan
    dotnet ef migrations list --project .\src\PeluqueriaSaaS.Infrastructure --startup-project .\src\PeluqueriaSaaS.Web
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ MIGRACIONES: Comando exitoso - verificar si 'AddModuloPOS' aparece en lista" -ForegroundColor Green
    } else {
        Write-Host "❌ MIGRACIONES: ERROR - Problema con EF o BD" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ MIGRACIONES: EXCEPCIÓN - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🔍 PASO 3: Verificando estado base de datos..." -ForegroundColor Yellow
Write-Host "   📝 EJECUTAR MANUALMENTE estas consultas SQL:" -ForegroundColor Cyan
Write-Host ""
Write-Host "   -- Verificar tablas existentes intactas" -ForegroundColor Gray
Write-Host "   SELECT COUNT(*) as EmpleadosActivos FROM Empleados WHERE EsActivo = 1;" -ForegroundColor White
Write-Host "   SELECT COUNT(*) as ServiciosActivos FROM Servicios WHERE EsActivo = 1;" -ForegroundColor White
Write-Host ""
Write-Host "   -- Verificar si nuevas tablas fueron creadas" -ForegroundColor Gray
Write-Host "   SELECT CASE WHEN OBJECT_ID('Ventas') IS NOT NULL THEN 'EXISTE' ELSE 'NO EXISTE' END as TablaVentas;" -ForegroundColor White
Write-Host "   SELECT CASE WHEN OBJECT_ID('VentaDetalles') IS NOT NULL THEN 'EXISTE' ELSE 'NO EXISTE' END as TablaVentaDetalles;" -ForegroundColor White

Write-Host ""
Write-Host "🔍 PASO 4: Intentar ejecutar aplicación..." -ForegroundColor Yellow
Write-Host "   📝 EJECUTAR MANUALMENTE:" -ForegroundColor Cyan
Write-Host "   dotnet run --project .\src\PeluqueriaSaaS.Web" -ForegroundColor White
Write-Host ""
Write-Host "   🎯 SI EJECUTA OK:" -ForegroundColor Green
Write-Host "     - Verificar http://localhost:5043/Empleados" -ForegroundColor White
Write-Host "     - Verificar http://localhost:5043/Servicios" -ForegroundColor White
Write-Host ""
Write-Host "   🚨 SI NO EJECUTA:" -ForegroundColor Red
Write-Host "     - Anotar error exacto mostrado" -ForegroundColor White
Write-Host "     - NO intentar 'fixes' automáticos" -ForegroundColor White

Write-Host ""
Write-Host "📊 POSIBLES ESCENARIOS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "🟢 ESCENARIO 1: Todo OK" -ForegroundColor Green
Write-Host "   - Build OK, App ejecuta, URLs funcionan, tablas nuevas creadas" -ForegroundColor White
Write-Host "   - ✅ ACCIÓN: Continuar con ITERACIÓN 2" -ForegroundColor Green
Write-Host ""
Write-Host "🟡 ESCENARIO 2: Migración parcial" -ForegroundColor Yellow
Write-Host "   - Build OK, App ejecuta, URLs funcionan, tablas nuevas NO creadas" -ForegroundColor White
Write-Host "   - ✅ ACCIÓN: Re-ejecutar solo migración EF" -ForegroundColor Yellow
Write-Host ""
Write-Host "🟠 ESCENARIO 3: DbContext roto" -ForegroundColor DarkYellow
Write-Host "   - Build ERROR, problema compilación" -ForegroundColor White
Write-Host "   - ✅ ACCIÓN: Rollback cambios DbContext" -ForegroundColor DarkYellow
Write-Host ""
Write-Host "🔴 ESCENARIO 4: BD corrupta" -ForegroundColor Red
Write-Host "   - Build OK, App NO ejecuta, errores BD" -ForegroundColor White
Write-Host "   - ✅ ACCIÓN: Restore desde backup_001.sql" -ForegroundColor Red

Write-Host ""
Write-Host "💬 REPORTAR RESULTADO EXACTO:" -ForegroundColor Cyan
Write-Host "   - Build: [OK/ERROR + detalles]" -ForegroundColor White
Write-Host "   - Migraciones EF: [lista exacta que aparece]" -ForegroundColor White
Write-Host "   - Tablas BD: [resultados consultas SQL]" -ForegroundColor White
Write-Host "   - App ejecuta: [SÍ/NO + error si aplica]" -ForegroundColor White
Write-Host "   - URLs funcionan: [OK/ERROR en cual]" -ForegroundColor White
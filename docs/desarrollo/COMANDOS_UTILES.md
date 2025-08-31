# 🔨 COMANDOS ÚTILES - PeluqueriaSaaS

## Comandos frecuentes para desarrollo y mantenimiento

---

## 📦 .NET Development

### Compilación y Ejecución
```bash
# Compilar solución completa
dotnet build

# Compilar proyecto específico
dotnet build src/PeluqueriaSaaS.Web

# Ejecutar aplicación web
dotnet run --project src/PeluqueriaSaaS.Web

# Ejecutar con hot reload
dotnet watch run --project src/PeluqueriaSaaS.Web

# Limpiar archivos compilados
dotnet clean

# Restaurar paquetes NuGet
dotnet restore
```

### Publicación
```bash
# Publicar para producción
dotnet publish -c Release -o ./publish

# Publicar self-contained
dotnet publish -c Release -r win-x64 --self-contained true
```

---

## 🗄️ Entity Framework Core

### Migraciones (NO USAR - Solo referencia)
```bash
# NO USAR EN ESTE PROYECTO - Usamos SQL manual
# Estos comandos son solo para referencia si algún día cambiamos de estrategia

# dotnet ef migrations add NombreMigracion -p src/PeluqueriaSaaS.Infrastructure -s src/PeluqueriaSaaS.Web
# dotnet ef database update -p src/PeluqueriaSaaS.Infrastructure -s src/PeluqueriaSaaS.Web
# dotnet ef migrations remove -p src/PeluqueriaSaaS.Infrastructure -s src/PeluqueriaSaaS.Web
```

### Inspección de BD
```bash
# Ver el modelo actual de EF
dotnet ef dbcontext info -p src/PeluqueriaSaaS.Infrastructure -s src/PeluqueriaSaaS.Web

# Generar script SQL del modelo
dotnet ef dbcontext script -p src/PeluqueriaSaaS.Infrastructure -s src/PeluqueriaSaaS.Web
```

---

## 💾 SQL Server

### Backup Base de Datos
```sql
-- Backup completo con fecha
BACKUP DATABASE PeluqueriaSaaSDB 
TO DISK = 'C:\Backups\PeluqueriaSaaS_20250829.bak'
WITH FORMAT, INIT, NAME = 'Full Backup PeluqueriaSaaS';

-- Backup antes de cambios importantes
DECLARE @BackupFile NVARCHAR(500)
SET @BackupFile = 'C:\Backups\PeluqueriaSaaS_' + 
                  FORMAT(GETDATE(), 'yyyyMMdd_HHmmss') + '.bak'
EXEC('BACKUP DATABASE PeluqueriaSaaSDB TO DISK = ''' + @BackupFile + '''')
```

### Restore Base de Datos
```sql
-- Restore desde backup
RESTORE DATABASE PeluqueriaSaaSDB 
FROM DISK = 'C:\Backups\PeluqueriaSaaS_20250829.bak'
WITH REPLACE;

-- Ver backups disponibles
RESTORE HEADERONLY 
FROM DISK = 'C:\Backups\PeluqueriaSaaS_20250829.bak';
```

### Queries Útiles de Mantenimiento
```sql
-- Ver tamaño de tablas
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.is_ms_shipped = 0
GROUP BY t.Name, p.Rows
ORDER BY TotalSpaceKB DESC;

-- Ver conexiones activas
SELECT * FROM sys.dm_exec_sessions WHERE database_id = DB_ID('PeluqueriaSaaSDB');

-- Limpiar log de transacciones
DBCC SHRINKFILE (PeluqueriaSaaSDB_log, 1);
```

---

## 📊 Queries de Negocio

### Dashboard y Reportes
```sql
-- Ventas del día
SELECT COUNT(*) as CantidadVentas, SUM(Total) as TotalVentas
FROM Ventas 
WHERE CAST(FechaVenta as DATE) = CAST(GETDATE() as DATE)
AND TenantId = 'default';

-- Top 5 servicios más vendidos del mes
SELECT TOP 5 
    s.Nombre,
    COUNT(*) as Cantidad,
    SUM(vd.Subtotal) as Total
FROM VentaDetalles vd
INNER JOIN Servicios s ON vd.ServicioId = s.Id
INNER JOIN Ventas v ON vd.VentaId = v.VentaId
WHERE MONTH(v.FechaVenta) = MONTH(GETDATE())
AND YEAR(v.FechaVenta) = YEAR(GETDATE())
GROUP BY s.Nombre
ORDER BY Cantidad DESC;

-- Empleados más productivos
SELECT 
    e.Nombre + ' ' + e.Apellido as Empleado,
    COUNT(DISTINCT v.VentaId) as VentasRealizadas,
    SUM(v.Total) as TotalGenerado
FROM Ventas v
INNER JOIN Empleados e ON v.EmpleadoId = e.Id
WHERE v.FechaVenta >= DATEADD(MONTH, -1, GETDATE())
GROUP BY e.Id, e.Nombre, e.Apellido
ORDER BY TotalGenerado DESC;
```

### Mantenimiento de Datos
```sql
-- Verificar empleado sistema
SELECT * FROM Empleados WHERE Email = 'sistema@peluqueria.com';

-- Verificar cliente ocasional
SELECT * FROM Clientes WHERE Nombre = 'CLIENTE' AND Apellido = 'OCASIONAL';

-- Ver últimos comprobantes generados
SELECT TOP 10 * FROM Comprobantes 
ORDER BY ComprobanteId DESC;

-- Verificar integridad referencial
SELECT c.ComprobanteId, c.ClienteId, cl.Nombre, c.EmpleadoId, e.Nombre
FROM Comprobantes c
LEFT JOIN Clientes cl ON c.ClienteId = cl.Id
LEFT JOIN Empleados e ON c.EmpleadoId = e.Id
WHERE c.FechaCreacion >= DATEADD(DAY, -7, GETDATE());
```

---

## 🐙 Git

### Comandos Básicos
```bash
# Ver estado
git status

# Ver cambios no commiteados
git diff

# Agregar todos los cambios
git add .

# Commit con mensaje descriptivo
git commit -m "tipo: descripción breve

Descripción detallada de los cambios realizados.
- Cambio 1
- Cambio 2

RESUMEN_XXX_MAESTRO.md actualizado"

# Push a repositorio
git push origin main

# Pull últimos cambios
git pull origin main
```

### Formato de Commits
```bash
# Tipos de commit:
# feat:     Nueva funcionalidad
# fix:      Corrección de bug
# docs:     Cambios en documentación
# style:    Formato, punto y coma faltantes, etc
# refactor: Refactorización de código
# test:     Agregar tests faltantes
# chore:    Mantenimiento, actualización de dependencias

# Ejemplos:
git commit -m "feat: Agregar referencias empleado a comprobantes"
git commit -m "fix: Corregir modal estadísticas que no cierra"
git commit -m "docs: Actualizar RESUMEN_067_MAESTRO con cambios"
```

### Gestión de Branches
```bash
# Crear y cambiar a nueva rama
git checkout -b feature/nombre-feature

# Cambiar entre ramas
git checkout main
git checkout feature/nombre-feature

# Merge de rama
git checkout main
git merge feature/nombre-feature

# Eliminar rama local
git branch -d feature/nombre-feature
```

---

## 🖥️ PowerShell / Terminal

### Navegación y Archivos
```powershell
# Navegar al proyecto
cd C:\Users\marce\source\repos\PeluqueriaSaaS

# Ver estructura de carpetas
tree /f

# Buscar texto en archivos
Get-ChildItem -Recurse *.cs | Select-String "EmpleadoId"

# Copiar con fecha
$fecha = Get-Date -Format "yyyyMMdd"
Copy-Item "archivo.bak" "archivo_$fecha.bak"
```

### Procesos y Puertos
```powershell
# Ver proceso usando puerto 5043
netstat -ano | findstr :5043

# Matar proceso por PID
taskkill /PID <PID> /F

# Ver todos los procesos dotnet
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}
```

---

## 🔧 Visual Studio

### Atajos de Teclado Útiles
```
Ctrl + K, Ctrl + D    - Formatear documento
Ctrl + K, Ctrl + C    - Comentar líneas
Ctrl + K, Ctrl + U    - Descomentar líneas
F12                   - Ir a definición
Ctrl + F12            - Ir a implementación
Shift + F12           - Buscar todas las referencias
Ctrl + .              - Quick actions
F5                    - Ejecutar con debug
Ctrl + F5             - Ejecutar sin debug
Ctrl + Shift + B      - Compilar solución
```

---

## 🐛 Debugging

### Logs y Diagnóstico
```csharp
// En Controllers o Services
_logger.LogInformation("Mensaje informativo");
_logger.LogWarning("Advertencia");
_logger.LogError(ex, "Error con excepción");

// Console para debug rápido
Console.WriteLine($"DEBUG: Variable = {variable}");
System.Diagnostics.Debug.WriteLine("DEBUG: Mensaje");
```

### SQL Profiler Queries
```sql
-- Ver queries ejecutadas recientemente
SELECT 
    deqs.creation_time,
    dest.text as Query,
    deqs.execution_count
FROM sys.dm_exec_query_stats deqs
CROSS APPLY sys.dm_exec_sql_text(deqs.sql_handle) dest
ORDER BY deqs.creation_time DESC;
```

---

## 📝 Documentación

### Generar nuevo resumen
```markdown
# Plantilla para RESUMEN_XXX_MAESTRO.md

1. Copiar el resumen anterior
2. Actualizar número de resumen
3. Documentar cambios realizados
4. Actualizar estado del sistema
5. Agregar problemas encontrados
6. Actualizar próximos pasos
```

### Actualizar Project Knowledge
```
1. Exportar RESUMEN_XXX_MAESTRO.md actualizado
2. Si hay cambios arquitectónicos, actualizar ARQUITECTURA_premisas_inmutables.md
3. Si hay nuevos problemas, actualizar DESARROLLO_problemas_conocidos.md
4. Si cambió BD, exportar nuevo script
```

---

## 🚀 Deployment (Futuro)

### IIS (Pendiente configurar)
```powershell
# Publicar a IIS
dotnet publish -c Release -o C:\inetpub\wwwroot\PeluqueriaSaaS

# Reciclar Application Pool
& "$env:windir\system32\inetsrv\appcmd.exe" recycle apppool /apppool.name:"PeluqueriaSaaS"
```

---

## 📊 Métricas del Proyecto

### Contar líneas de código
```powershell
# Total líneas en archivos .cs
(Get-ChildItem -Path . -Filter *.cs -Recurse | Get-Content | Measure-Object -Line).Lines

# Por tipo de archivo
Get-ChildItem -Recurse -Include *.cs,*.cshtml,*.js,*.css | 
    Group-Object Extension | 
    Select Name, @{Name="Lines";Expression={($_.Group | Get-Content | Measure-Object -Line).Lines}}
```

---

**Documento actualizado:** Agosto 2025  
**Útil para:** Desarrollo diario, mantenimiento, debugging  
**Mantener actualizado:** Cuando se agreguen nuevos comandos frecuentes
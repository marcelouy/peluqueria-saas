# 🐛 Problemas Conocidos y Soluciones

## Activos (Pendientes de resolver):

### 1. Modal Estadísticas No Cierra
- **Síntoma:** Click en X no cierra modal
- **Workaround:** Refresh página
- **Archivo:** dashboard.js

### 2. Dropdown Cargo en Edit Empleado
- **Síntoma:** No carga opciones
- **Workaround:** Editar directo en BD
- **Archivo:** Views/Empleados/Edit.cshtml

## Resueltos:

### 1. Empleado #40 como texto ✅
- **Resumen:** #67
- **Solución:** Referencias reales con EmpleadoId

### 2. Mapeo EF Core Comprobantes ✅
- **Resumen:** #64-65
- **Solución:** SQL directo en repository

### 3. TenantId Inconsistente ✅
- **Resumen:** #65
- **Solución:** Unificar a "default"
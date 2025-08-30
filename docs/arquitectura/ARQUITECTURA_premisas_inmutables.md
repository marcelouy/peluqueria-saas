# 🚨 PREMISAS INMUTABLES - NUNCA CAMBIAR

## Reglas de Oro:

### 1. NO MODIFICAR
- EntityBase.cs
- TenantEntityBase.cs
- Propiedades private set sin razón válida

### 2. NO USAR
- Entity Framework Migrations (usar SQL manual)
- IDs hardcodeados (buscar por claves naturales)
- Procedimientos almacenados

### 3. SIEMPRE
- TenantId = "default"
- Backup antes de cambios BD
- Documentar en resumen maestro
- Probar en desarrollo primero

### 4. ARQUITECTURA
- Clean Architecture estricta
- Repository Pattern
- Domain-Driven Design
- Entidades inmutables

## Valores del Sistema:
- DEFAULT_TENANT_ID = "default"
- DEFAULT_SERIE = "A001"
- DEFAULT_USUARIO = "María González"
- EMPLEADO_SISTEMA_EMAIL = "sistema@peluqueria.com"
# 📊 ESQUEMA BASE DE DATOS - PeluqueriaSaaS
## Estado actual: Agosto 2025

---

## 📋 RESUMEN DE TABLAS

Total de tablas: **24**

### Tablas Core del Negocio
1. **Empleados** - Personal del sistema
2. **Clientes** - Clientes del negocio
3. **Servicios** - Catálogo de servicios
4. **Ventas** - Transacciones principales
5. **VentaDetalles** - Líneas de venta
6. **Comprobantes** - Documentos fiscales
7. **ComprobanteDetalles** - Líneas de comprobante

### Tablas de Configuración
8. **Settings** - Configuración general
9. **ConfiguracionComprobantes** - Config específica comprobantes
10. **TiposServicio** - Categorías de servicios
11. **EstadoServicio** - Estados posibles de servicios

### Tablas de Estructura Organizacional
12. **Empresas** - Empresa principal
13. **Sucursales** - Sucursales de la empresa
14. **Estaciones** - Estaciones de trabajo

### Tablas de Gestión de Citas
15. **Citas** - Citas agendadas
16. **CitaServicios** - Servicios por cita
17. **HistorialCliente** - Historial de visitas

### Tablas de Inventario
18. **Articulos** - Productos en inventario

### Tablas de Impuestos
19. **TiposImpuestos** - Tipos de impuestos
20. **TasasImpuestos** - Tasas específicas
21. **ArticulosImpuestos** - Impuestos por artículo
22. **ServiciosImpuestos** - Impuestos por servicio
23. **HistoricoTasasImpuestos** - Histórico de cambios

### Tablas Obsoletas/Sin usar
24. **CitaEstacion** (mencionada en Entity pero no en BD)
25. **EmpleadoEstacion** (mencionada en Entity pero no en BD)

---

## 🔑 TABLAS PRINCIPALES DETALLADAS

### 1. **Empleados** (57+ registros)
```sql
- Id (PK, IDENTITY)
- Nombre, Apellido (NOT NULL)
- Email (NULL) - ÚNICO para sistema@peluqueria.com
- Telefono, FechaNacimiento (NULL)
- Cargo (NOT NULL, DEFAULT 'Estilista')
- Salario (decimal 18,2)
- FechaContratacion, FechaIngreso, FechaRegistro
- Direccion, Ciudad, CodigoPostal
- SucursalId (FK -> Sucursales)
- TenantId (DEFAULT 'default')
- EsActivo, Activo (bits)
- Campos auditoría (FechaCreacion, etc.)
```
**Especial:** Empleado Sistema con email sistema@peluqueria.com

### 2. **Clientes**
```sql
- Id (PK, IDENTITY)
- Nombre, Apellido (NOT NULL)
- Email, Telefono (NULL)
- FechaNacimiento (NULL)
- Notas (nvarchar 1000)
- TenantId (DEFAULT 'default')
- EsActivo, Activo (bits)
- Campos auditoría
```
**Especial:** Cliente "CLIENTE OCASIONAL" para walk-ins

### 3. **Ventas**
```sql
- VentaId (PK, IDENTITY)
- NumeroVenta (UNIQUE, nvarchar 20)
- FechaVenta
- EmpleadoId (FK -> Empleados, NOT NULL)
- ClienteId (FK -> Clientes, NOT NULL)
- SubTotal, Descuento, Total (decimal 10,2)
- EstadoVenta (DEFAULT 'Completada')
- Observaciones (nvarchar 500)
- TenantId (DEFAULT 'default')
```

### 4. **VentaDetalles**
```sql
- VentaDetalleId (PK, IDENTITY)
- VentaId (FK -> Ventas, CASCADE DELETE)
- ServicioId (FK -> Servicios)
- NombreServicio, PrecioUnitario, Cantidad, Subtotal
- EmpleadoServicioId (FK -> Empleados, NULL)
- EstadoServicioId (FK -> EstadoServicio)
- EmpleadoAsignadoId (NULL)
- InicioServicio, FinServicio (datetime2, NULL)
```

### 5. **Comprobantes** (Modificado en #67)
```sql
- ComprobanteId (PK, IDENTITY)
- VentaId (NOT NULL)
- Serie (varchar 10), Numero (int)
- FechaEmision
- ClienteNombre (nvarchar 400)
- ClienteDocumento (varchar 20, NULL)
- ClienteId (INT NULL) -- NUEVO en #67
- EmpleadoId (INT NOT NULL, DEFAULT 50) -- NUEVO en #67
- SubTotal, Descuento, Impuestos, Total
- MetodoPago, Estado (DEFAULT 'EMITIDO')
- MotivoAnulacion, FechaAnulacion, UsuarioAnulacion
- UNIQUE (Serie, Numero, TenantId)
```

### 6. **ComprobanteDetalles** (Modificado en #67)
```sql
- DetalleId (PK, IDENTITY)
- ComprobanteId (FK -> Comprobantes, CASCADE DELETE)
- TipoItem (varchar 20)
- ItemId (int, NULL)
- Descripcion, Cantidad, PrecioUnitario
- Descuento, Impuestos, Total
- EmpleadoNombre (nvarchar 200, NULL)
- EmpleadoId (INT NULL) -- NUEVO en #67
```

### 7. **Servicios**
```sql
- Id (PK, IDENTITY)
- Nombre (nvarchar 100)
- Descripcion (nvarchar 500)
- DuracionMinutos (int)
- Precio (decimal 18,2)
- Moneda (DEFAULT 'UYU')
- TipoServicioId (FK -> TiposServicio)
- TenantId (DEFAULT 'default')
```

### 8. **Settings** (1 registro por tenant)
```sql
- Id (PK, IDENTITY)
- CodigoPeluqueria (UNIQUE, DEFAULT 'MAIN')
- NombrePeluqueria, DireccionPeluqueria, TelefonoPeluqueria
- ResumenEncabezado (DEFAULT 'COMPROBANTE INTERNO - SIN VALOR FISCAL')
- SerieComprobante (DEFAULT 'CI')
- UltimoNumeroComprobante (int)
- Configuraciones de impresión y display
```

---

## 📊 TABLAS DE IMPUESTOS

### **TiposImpuestos**
```sql
- Id (PK)
- Codigo (UNIQUE)
- Nombre, Descripcion
- TipoCalculo (DEFAULT 'PORCENTAJE')
- AplicaA (DEFAULT 'AMBOS')
- OrdenAplicacion
```

### **TasasImpuestos**
```sql
- Id (PK)
- TipoImpuestoId (FK)
- Porcentaje (CHECK 0-100)
- FechaInicio, FechaFin
- EsTasaPorDefecto
```

### **ArticulosImpuestos** / **ServiciosImpuestos**
```sql
- Id (PK)
- ArticuloId/ServicioId (FK, CASCADE DELETE)
- TasaImpuestoId (FK)
- FechaInicioAplicacion, FechaFinAplicacion
- UNIQUE (Item, Tasa, Tenant) WHERE Activo=1
```

---

## 🏢 TABLAS ORGANIZACIONALES

### **Empresas**
```sql
- Id (PK)
- Nombre, RUT, Direccion, Telefono, Email
- PlanSuscripcion (DEFAULT 'BASICO')
- HorarioAperturaDefecto (DEFAULT '09:00')
- HorarioCierreDefecto (DEFAULT '18:00')
```

### **Sucursales**
```sql
- Id (PK)
- Nombre, Direccion, Telefono
- EmpresaId (FK -> Empresas)
- TenantId (DEFAULT 'default')
```

### **Estaciones**
```sql
- Id (PK)
- Nombre, Descripcion
- TipoEstacionId (NULL)
- SucursalId (FK -> Sucursales)
- Capacidad (DEFAULT 1)
```

---

## 📅 TABLAS DE CITAS

### **Citas**
```sql
- Id (PK)
- FechaHora
- ClienteId (FK -> Clientes)
- EmpleadoId (FK -> Empleados, NULL)
- SucursalId (FK -> Sucursales)
- MontoTotal, MontoPagado
- EstadoCitaId (NULL)
```

### **CitaServicios**
```sql
- Id (PK)
- CitaId (FK -> Citas, CASCADE DELETE)
- ServicioId (FK -> Servicios)
- EmpleadoId (FK -> Empleados, NULL)
- PrecioFinal, DuracionMinutos
```

---

## 🔧 ÍNDICES IMPORTANTES

### Índices de Performance
- `IX_Ventas_FechaVenta_TenantId`
- `IX_Ventas_NumeroVenta` (UNIQUE)
- `IX_Comprobantes_VentaId`
- `IX_Comprobantes_FechaEmision`
- `IX_ComprobanteDetalles_ComprobanteId`

### Índices de Referencias (#67)
- `IX_Comprobantes_ClienteId`
- `IX_Comprobantes_EmpleadoId`
- `IX_ComprobanteDetalles_EmpleadoId`

### Índices Multi-tenant
- `IX_[Tabla]_TenantId` en casi todas las tablas

---

## 🔄 FOREIGN KEYS PRINCIPALES

### Con CASCADE DELETE
- VentaDetalles -> Ventas
- ComprobanteDetalles -> Comprobantes
- CitaServicios -> Citas
- ArticulosImpuestos -> Articulos
- ServiciosImpuestos -> Servicios

### Sin CASCADE (referencias suaves)
- Comprobantes -> Clientes (NULL permitido)
- Comprobantes -> Empleados (NOT NULL)
- Ventas -> Clientes
- Ventas -> Empleados

---

## 📈 ESTADÍSTICAS

- **Total de tablas:** 24
- **Tablas con TenantId:** 20
- **Tablas con auditoría completa:** 22
- **Tablas con IDENTITY:** 23
- **Relaciones con CASCADE:** 5
- **Índices totales:** 40+
- **Constraints CHECK:** 1 (Porcentaje 0-100)

---

## 🚨 NOTAS IMPORTANTES

1. **EmpleadoId en Comprobantes:** DEFAULT 50 puede variar según instalación
2. **TenantId:** Siempre 'default' hasta implementar multi-tenant real
3. **Campos duplicados:** Algunos campos como EsActivo/Activo están duplicados
4. **Sin FKs estrictas:** Comprobantes maneja referencias sin constraints para flexibilidad
5. **Numeración:** Cada tenant tiene su propia secuencia de comprobantes

---

**Script completo:** Ver `/docs/base-datos/scripts/001_schema_completo_2025-08-29.sql`  
**Última modificación estructural:** Resumen #67 (ClienteId y EmpleadoId en Comprobantes)
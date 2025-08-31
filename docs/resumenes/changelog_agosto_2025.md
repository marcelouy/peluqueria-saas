# 📝 CHANGELOG - Agosto 2025
## Sistema PeluqueriaSaaS - Registro de Cambios

---

## [v0.95.0] - 31-08-2025

### ✅ Added (Agregado)
- Sistema de referencias reales empleado/cliente en comprobantes
- Empleado sistema automático (se auto-crea si no existe)
- Métodos `GetByEmailAsync()` y `CreateSistemaAsync()` en EmpleadoRepository
- Modal de estadísticas en _Layout.cshtml
- PrepararDropdownData() en EmpleadosController
- Índices en BD para ClienteId y EmpleadoId en Comprobantes

### 🔧 Fixed (Corregido)
- Dropdown cargo/horario en Edit Empleado ahora funciona
- Cliente ocasional duplicado ("Walk-in") eliminado
- Modal estadísticas cierra correctamente (X, ESC, click fuera)
- EmpleadosController restaurado con todos los métodos
- Referencias empleado como texto convertidas a IDs reales
- IDs hardcodeados eliminados (ahora búsqueda por email/nombre)

### 📊 Changed (Modificado)
- ComprobanteService usa empleado sistema dinámico
- VentasController no agrega "Walk-in" manualmente
- Comprobantes ahora con ClienteId (nullable) y EmpleadoId (required)
- ComprobanteDetalles con EmpleadoId opcional

### 🗑️ Removed (Eliminado)
- Cliente "Walk-in" hardcodeado
- ID 50 hardcodeado para empleado
- Lógica duplicada de cliente ocasional

---

## [v0.93.0] - 30-08-2025

### ✅ Added
- Sistema de comprobantes completo
- Numeración automática de comprobantes
- Serie configurable (A001)
- Integración comprobantes con ventas

### 🔧 Fixed
- Constructores de Comprobante
- Mapeo EF Core para comprobantes
- Relaciones FK en base de datos

---

## [v0.90.0] - 29-08-2025

### ✅ Added
- Módulo de Empleados completo
- Módulo de Clientes unificado
- Sistema POS funcional
- Dashboard con Chart.js
- 7 servicios activos con categorías
- Sistema de impuestos configurable

### 📊 Initial Release Features
- Multi-tenant con TenantId = "default"
- Clean Architecture
- Repository Pattern
- Domain-Driven Design
- SQL Server database
- 19 tablas implementadas
- Sin Entity Framework Migrations (SQL manual)

---

## 📈 Estadísticas del Proyecto

### Progresión de Funcionalidad
- Chat #62: 75% funcional
- Chat #65: 85% funcional
- Chat #67: 93% funcional
- Chat #69: 95% funcional

### Bugs Resueltos
- Total: 54 bugs
- Críticos: 0 actuales
- Menores: 0 actuales

### Métricas de Código
- ~30,000 líneas de código
- 16 entidades de dominio
- 9 repositories
- 7 services
- 7 controllers
- 25+ vistas Razor

---

## 🔮 Roadmap Futuro

### v1.0.0 (Próximo)
- [ ] Sistema de reportes por empleado
- [ ] Cálculo de comisiones
- [ ] Exportación Excel/PDF
- [ ] Dashboard mejorado

### v1.1.0
- [ ] Sistema de citas/agenda
- [ ] Notificaciones automáticas
- [ ] Multi-sucursal real
- [ ] Programa de fidelidad

### v1.2.0
- [ ] App móvil
- [ ] API REST completa
- [ ] Integración con pasarelas de pago
- [ ] Sistema de facturación electrónica

---

## 📝 Notas de Versión

### Convención de Versionado
Seguimos [Semantic Versioning](https://semver.org/):
- MAJOR.MINOR.PATCH
- 0.x.x = Pre-release/Beta
- 1.0.0 = Primera versión estable

### Tags Git Recomendados
```bash
git tag -a v0.95.0 -m "Sistema estabilizado - 95% funcional"
git push origin v0.95.0
```

---

**Mantenido por:** Marcelo  
**Última actualización:** 31 de Agosto 2025
# 🏗️ DECISIONES ARQUITECTÓNICAS - PeluqueriaSaaS

## Registro de decisiones importantes y sus razones

---

## 1. Multi-tenant con TenantId
**Decisión:** Usar TenantId en todas las entidades  
**Fecha:** Inicio del proyecto  
**Razón:** Aislamiento total entre peluquerías para modelo SaaS  
**Estado actual:** TenantId = "default" hardcodeado  
**Futuro:** Resolver por subdominio o header HTTP  
**Impacto:** Todas las queries deben filtrar por TenantId  

---

## 2. Sin Entity Framework Migrations
**Decisión:** Usar SQL manual para todos los cambios de BD  
**Fecha:** Resumen #40  
**Razón:** Control total sobre esquema, evitar sorpresas de EF  
**Implementación:** Scripts numerados en /docs/base-datos/scripts/  
**Beneficio:** Predictibilidad y control fino  
**Costo:** Más trabajo manual, pero más seguro  

---

## 3. Empleado Sistema Dinámico
**Decisión:** Buscar por email, no por ID hardcodeado  
**Fecha:** Resumen #67  
**Razón:** IDs pueden cambiar entre instalaciones  
**Implementación:** sistema@peluqueria.com como clave natural  
**Beneficio:** Auto-reparación si no existe  
**Código:** GetEmpleadoSistemaIdAsync() con cache  

---

## 4. Cliente Ocasional
**Decisión:** Cliente por defecto para ventas walk-in  
**Fecha:** Resumen #30  
**Razón:** No todos requieren registro completo  
**Implementación:** Buscar por nombre "CLIENTE OCASIONAL"  
**Beneficio:** Flujo rápido de ventas  
**Trade-off:** Menos datos para marketing  

---

## 5. Repository Pattern Estricto
**Decisión:** Abstracción total de acceso a datos  
**Fecha:** Inicio del proyecto  
**Razón:** Flexibilidad para cambiar ORM o BD  
**Implementación:** Interfaces en Domain, implementación en Infrastructure  
**Excepción:** Cliente usa DbContext directo (no necesita repository)  
**Beneficio:** Testabilidad y mantenibilidad  

---

## 6. Entidades Inmutables
**Decisión:** Propiedades con private set, modificación por métodos  
**Fecha:** Inicio del proyecto  
**Razón:** Proteger invariantes del dominio  
**Implementación:** Constructores con validación, métodos de negocio  
**Ejemplo:** Venta.AgregarDetalle() en lugar de List.Add()  
**Beneficio:** Consistencia garantizada  

---

## 7. Value Objects para valores con lógica
**Decisión:** Encapsular valores que tienen comportamiento  
**Fecha:** Resumen #20  
**Implementación:** Dinero, Email, Telefono como Value Objects  
**Razón:** Evitar primitives obsession  
**Beneficio:** Validación centralizada  
**Costo:** Más complejidad inicial  

---

## 8. CQRS Light con MediatR
**Decisión:** Separar comandos y queries sin event sourcing  
**Fecha:** Resumen #25  
**Razón:** Separación de responsabilidades sin complejidad excesiva  
**Implementación:** Commands y Queries en Application  
**Beneficio:** Código más organizado  
**Trade-off:** Más archivos, pero más claro  

---

## 9. Sin IClienteRepository
**Decisión:** Usar DbContext directamente para Clientes  
**Fecha:** Resumen #57  
**Razón:** CRUD simple no justifica abstracción  
**Implementación:** ClientesController usa DbContext  
**Beneficio:** Menos código boilerplate  
**Riesgo:** Acoplamiento, pero aceptable para CRUD  

---

## 10. Comprobantes sin Foreign Keys en BD
**Decisión:** Relaciones manejadas en código, no en BD  
**Fecha:** Resumen #62  
**Razón:** Flexibilidad para datos históricos  
**Implementación:** ClienteId y EmpleadoId sin FK constraints  
**Beneficio:** No se rompe si se elimina un empleado  
**Trade-off:** Sin integridad referencial automática  

---

## 11. Numeración de Comprobantes por Tenant
**Decisión:** Cada tenant tiene su propia secuencia  
**Fecha:** Resumen #62  
**Implementación:** Serie + Numero únicos por TenantId  
**Razón:** Requisito legal/fiscal por empresa  
**Beneficio:** Independencia entre tenants  

---

## 12. JavaScript y CSS separados
**Decisión:** No bundling, archivos individuales en wwwroot  
**Fecha:** Inicio del proyecto  
**Razón:** Simplicidad de desarrollo y debugging  
**Implementación:** /wwwroot/js/ y /wwwroot/css/  
**Trade-off:** Más requests HTTP, pero más simple  
**Futuro:** Considerar bundling en producción  

---

## 13. Bootstrap + jQuery (no SPA)
**Decisión:** MVC tradicional con Razor Views  
**Fecha:** Inicio del proyecto  
**Razón:** Rapidez de desarrollo, familiaridad  
**Implementación:** ASP.NET Core MVC + Ajax para interactividad  
**Trade-off:** Menos "moderno" pero más productivo  
**Beneficio:** Time to market más rápido  

---

## 14. Backup antes de cambios de BD
**Decisión:** Backup obligatorio antes de ALTER TABLE  
**Fecha:** Resumen #45 (después de perder datos)  
**Razón:** Seguridad y posibilidad de rollback  
**Implementación:** Scripts en /src/Artefactos/Respaldos/  
**Frecuencia:** Antes de cada cambio estructural  

---

## 15. Empleado siempre asignado a venta
**Decisión:** EmpleadoId NOT NULL en Ventas  
**Fecha:** Resumen #40  
**Razón:** Responsabilidad y comisiones  
**Implementación:** Empleado Sistema si no hay asignado  
**Beneficio:** Trazabilidad completa  

---

## 16. Estados de servicio configurables
**Decisión:** Tabla EstadoServicio parametrizable  
**Fecha:** Resumen #59  
**Razón:** Flexibilidad por negocio  
**Estados base:** Pendiente, EnProceso, Completado, Cancelado  
**Beneficio:** Adaptable a diferentes flujos  

---

## 17. Impuestos como entidades separadas
**Decisión:** TiposImpuestos + TasasImpuestos + relaciones  
**Fecha:** Resumen #53  
**Razón:** Cambios fiscales frecuentes en Uruguay  
**Implementación:** Histórico de tasas, vigencias  
**Beneficio:** Auditoría completa  
**Complejidad:** Alta, pero necesaria  

---

## 18. Sin procedimientos almacenados
**Decisión:** Toda la lógica en C#  
**Fecha:** Inicio del proyecto  
**Razón:** Mantenibilidad, versionado, testing  
**Implementación:** Repository pattern + Services  
**Trade-off:** Potencial pérdida de performance  
**Beneficio:** Código en un solo lugar  

---

## 19. Datos de auditoría en todas las tablas
**Decisión:** FechaCreacion, CreadoPor, etc. en EntityBase  
**Fecha:** Inicio del proyecto  
**Razón:** Requisitos de auditoría y debugging  
**Implementación:** EntityBase con campos comunes  
**Costo:** Más espacio en BD  
**Beneficio:** Trazabilidad completa  

---

## 20. PuppeteerSharp para PDFs
**Decisión:** Generar PDFs via Chrome headless  
**Fecha:** Resumen #55  
**Razón:** Control total sobre diseño, HTML/CSS familiar  
**Implementación:** BrowserPool para performance  
**Trade-off:** Más recursos que libraries nativas  
**Beneficio:** Flexibilidad total en diseño  

---

## 📊 MÉTRICAS DE DECISIONES

- **Decisiones que funcionaron bien:** 15/20 (75%)
- **Decisiones que requirieron ajustes:** 4/20 (20%)
- **Decisiones pendientes de validar:** 1/20 (5%)
- **Deuda técnica generada:** Media-baja
- **Complejidad agregada:** Aceptable
- **Velocidad de desarrollo:** Alta

---

## 🔮 DECISIONES FUTURAS PENDIENTES

1. **Autenticación y autorización** - Identity vs JWT vs Auth0
2. **Caché distribuido** - Redis vs In-Memory
3. **Búsqueda full-text** - SQL Server FTS vs Elasticsearch
4. **Multi-tenant real** - Subdominio vs Header vs Base URL
5. **API REST** - Para app móvil futura
6. **Notificaciones real-time** - SignalR vs WebSockets
7. **Colas de mensajes** - Para procesos largos
8. **CDN para assets** - CloudFlare vs Azure CDN

---

## 📝 PLANTILLA PARA NUEVAS DECISIONES

```markdown
## N. [Título de la decisión]
**Decisión:** [Qué se decidió]
**Fecha:** [Resumen #XX o fecha]
**Razón:** [Por qué se tomó esta decisión]
**Implementación:** [Cómo se implementó]
**Beneficio:** [Qué se ganó]
**Trade-off:** [Qué se sacrificó]
**Estado:** [Activo/Deprecado/En revisión]
```

---

**Documento actualizado:** Agosto 2025  
**Última decisión:** #20 (PuppeteerSharp)  
**Próxima revisión:** Al llegar a 30 decisiones
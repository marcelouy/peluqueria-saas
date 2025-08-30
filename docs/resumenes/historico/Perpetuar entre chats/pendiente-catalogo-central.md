# 🎯 PENDIENTE ESTRATÉGICO: CATÁLOGO CENTRAL DE PRODUCTOS

## 📅 Fecha Registro: Diciembre 2024
## 🔥 Prioridad: ALTA (Diferenciador competitivo)
## 💰 Impacto en Revenue: MUY ALTO
## ⏱️ Estimación: 2-3 sprints

---

## 🎯 CONCEPTO DE NEGOCIO

Sistema de **catálogo central de productos** gestionado por el SaaS que permite:
- Mantener una base de datos central de productos de proveedores principales
- Actualización centralizada de precios
- Distribución automática a clientes (peluquerías)
- Combinación con productos propios de cada empresa

## 🏗️ ARQUITECTURA PROPUESTA

### Nivel 1: CATÁLOGO CENTRAL (Gestión SaaS)
```
ProductosCentrales
├── Productos de marcas profesionales (L'Oreal, Wella, etc.)
├── Precios negociados con proveedores
├── Fichas técnicas y imágenes oficiales
├── Actualizaciones masivas de precios
└── Control de discontinuados
```

### Nivel 2: CATÁLOGO LOCAL (Por Empresa/Tenant)
```
ArticulosEmpresa
├── Productos sincronizados del catálogo central
├── Productos propios/exclusivos
├── Precios personalizados
├── Márgenes individuales
└── Gestión de stock local
```

## 💡 MODELO DE NEGOCIO

### Monetización Adicional:
1. **Comisiones de proveedores** por ventas a través de la plataforma
2. **Plan Premium** con acceso a catálogo actualizado
3. **Analytics para proveedores** (datos de consumo)
4. **Marketplace B2B** futuro

### Ventajas Competitivas:
- **Lock-in effect:** Difícil migrar con catálogo integrado
- **Economía de escala:** Mejores precios por volumen
- **Diferenciador único** vs competencia actual
- **Data valiosa** de consumo del mercado

## 📊 DISEÑO TÉCNICO PRELIMINAR

### Tablas Nuevas Requeridas:

```sql
-- Tabla de productos del catálogo central
CREATE TABLE CatalogoProductosCentral (
    Id INT PRIMARY KEY,
    SKU NVARCHAR(50) UNIQUE,
    CodigoBarras NVARCHAR(50),
    Nombre NVARCHAR(200),
    Marca NVARCHAR(100),
    ProveedorId INT,
    Categoria NVARCHAR(100),
    PrecioCostoBase DECIMAL(18,2),
    PrecioSugeridoVenta DECIMAL(18,2),
    Descripcion NVARCHAR(MAX),
    FichaTecnica NVARCHAR(MAX),
    ImagenPrincipalURL NVARCHAR(500),
    Activo BIT,
    FechaUltimaActualizacion DATETIME2
);

-- Relación con artículos locales
CREATE TABLE ArticulosCatalogoRelacion (
    ArticuloId INT,           -- Artículo local
    ProductoCentralId INT,    -- Producto del catálogo
    TenantId NVARCHAR(50),
    UsaPrecioCentral BIT,     -- Si sincroniza precios
    MargenPersonalizado DECIMAL(5,2),
    PRIMARY KEY (ArticuloId, TenantId)
);

-- Histórico de cambios de precios centrales
CREATE TABLE HistoricoPreciosCentrales (
    Id INT PRIMARY KEY,
    ProductoCentralId INT,
    PrecioCostoAnterior DECIMAL(18,2),
    PrecioCostoNuevo DECIMAL(18,2),
    FechaCambio DATETIME2,
    ProveedorNotificacion NVARCHAR(200)
);
```

### Modificaciones a Tablas Existentes:

```sql
ALTER TABLE Articulos ADD ProductoCentralId INT NULL;
ALTER TABLE Articulos ADD UsaPrecioCentral BIT DEFAULT 0;
ALTER TABLE Articulos ADD FechaUltimaSincronizacion DATETIME2 NULL;
```

## 🔄 FLUJO DE SINCRONIZACIÓN

1. **Actualización Central:**
   - Admin SaaS actualiza precios en catálogo central
   - Sistema registra cambios en histórico

2. **Propagación Automática:**
   - Notificación a empresas suscritas
   - Actualización automática si `UsaPrecioCentral = true`
   - Sugerencia de actualización si usa precio propio

3. **Gestión Local:**
   - Empresa puede aceptar/rechazar cambios
   - Mantener margen personalizado
   - Agregar productos propios

## 📈 MÉTRICAS DE ÉXITO

- **Adopción:** % empresas usando catálogo central
- **Sincronización:** Productos sincronizados vs locales
- **Revenue adicional:** Comisiones de proveedores
- **Engagement:** Frecuencia de actualizaciones aceptadas
- **Valor agregado:** Tiempo ahorrado en gestión de precios

## 🚀 PLAN DE IMPLEMENTACIÓN (FUTURO)

### Fase 1: MVP (Sprint 1)
- [ ] Diseño de tablas centrales
- [ ] API de sincronización básica
- [ ] UI para gestión central
- [ ] Import masivo de productos

### Fase 2: Integración (Sprint 2)
- [ ] Sincronización con artículos locales
- [ ] Sistema de notificaciones
- [ ] Dashboard de cambios de precios
- [ ] Histórico y auditoría

### Fase 3: Optimización (Sprint 3)
- [ ] Sincronización selectiva por categorías
- [ ] Reglas de margen automático
- [ ] Integración con proveedores vía API
- [ ] Analytics y reportes

## ⚠️ CONSIDERACIONES IMPORTANTES

### Técnicas:
- Necesitará **jobs de sincronización** periódicos
- **Caché** para catálogo central (Redis?)
- **API robusta** para actualizaciones masivas
- **Versionado** de cambios de precios

### Negocio:
- Requiere **acuerdos con proveedores**
- **Legal:** términos de uso de datos
- **Soporte:** capacitación a usuarios
- **Gradual:** empezar con pocos proveedores

## 💭 NOTAS PARA PRÓXIMO CHAT

```
IMPORTANTE: Este es un DIFERENCIADOR CLAVE del sistema.
- No es solo un "nice to have" - es estratégico
- Crea un moat competitivo difícil de replicar
- Genera revenue adicional sin costo marginal
- Aumenta el LTV de cada cliente

CUANDO IMPLEMENTAR:
- Después de tener 10+ clientes activos
- Con feedback validado del mercado
- Idealmente con 1-2 proveedores interesados
```

## 🔗 REFERENCIAS

- Modelo similar: **Salesforce Commerce Cloud**
- Competidor indirecto: **SAP Ariba** (B2B)
- Inspiración UX: **Shopify wholesale**

---

**🔒 ESTE PENDIENTE ES ESTRATÉGICO Y DEBE PERPETUARSE ENTRE CHATS**

*Actualizado: Diciembre 2024*
*Propuesto por: Usuario*
*Valor estimado: $$$$ (Muy Alto)*
*Complejidad: Media-Alta*
*ROI esperado: 300%+*
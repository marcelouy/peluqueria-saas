# 🚨 KNOWLEDGE BASE PERPETUO - PeluqueriaSaaS Architecture

**📅 FECHA:** Julio 31, 2025  
**🎯 PROPÓSITO:** Perpetuar conocimiento arquitectural + evitar errores repetidos  
**⚡ ESTADO:** CRÍTICO - Leer SIEMPRE antes de cualquier diseño  

---

## 🏗️ ARQUITECTURA ACTUAL CONFIRMADA

### **✅ PATTERNS ESTABLECIDOS - NUNCA CAMBIAR:**

**1. CATEGORIZACIÓN DINÁMICA (NO ENUMS ESTÁTICOS):**
```csharp
// ✅ CORRECTO - Entidad dinámica en BD
public class TipoServicio : EntityBase // int Id, string Nombre, etc.
public class Servicio { 
    public int TipoServicioId { get; private set; } // FK a tabla dinámica
    public virtual TipoServicio TipoServicio { get; private set; }
}

// ❌ INCORRECTO - NUNCA hacer esto
public enum TipoServicio { Corte = 1, Coloracion = 2 } // RÍGIDO + NO CONFIGURABLE
```

**2. MULTI-TENANT PATTERN:**
```csharp
// ✅ PATRÓN ESTABLECIDO
public abstract class TenantEntityBase : EntityBase {
    public string TenantId { get; private set; } = "default";
}
// Todas las entities heredan de TenantEntityBase
```

**3. VALUE OBJECTS PATTERN:**
```csharp
// ✅ PATRÓN ESTABLECIDO - NO usar decimal directo
public class Dinero { 
    public decimal Valor { get; }
    public string Moneda { get; } = "CLP"
}
// Servicio.Precio es Dinero, NO decimal
```

**4. RICH DOMAIN ENTITIES:**
```csharp
// ✅ PATRÓN ESTABLECIDO
public class Servicio : TenantEntityBase {
    public string Nombre { get; private set; } // Private setters
    private Servicio() { } // Private constructor
    public Servicio(params) { } // Public constructor with validation
    public void ActualizarInformacion(params) { } // Behavior methods
}
```

---

## 📊 ESTRUCTURA ACTUAL ENTITIES

### **🎯 ENTITIES EXISTENTES:**
- **Cliente** → TenantEntityBase + string properties (NO ValueObjects Email/Telefono)
- **Empleado** → Simple entity + NO inheritance + basic properties
- **Servicio** → TenantEntityBase + Dinero Precio + TipoServicioId FK
- **TipoServicio** → EntityBase + dynamic categories
- **Venta** → Simple entity + decimal properties + navigation
- **VentaDetalle** → Simple entity + FK Servicio + snapshot prices

### **🔄 PATTERNS INCONSISTENCIAS:**
- **Empleado** NO hereda TenantEntityBase (inconsistencia)
- **Venta/VentaDetalle** usan decimal directo (NO Dinero ValueObject)
- **Cliente** NO usa ValueObjects Email/Telefono (simplificado)

---

## 🚨 REGLAS PERPETUAS - NUNCA VIOLAR

### **❌ NUNCA HACER:**
1. **Enums para categorías** → Usar entidades dinámicas como TipoServicio
2. **Hardcodear estados** → Crear EntidadEstado dinámicas
3. **Ignorar multi-tenant** → Siempre considerar TenantId pattern
4. **Breaking changes** → Extender, nunca modificar estructura existente
5. **Decimal directo precios** → Usar Dinero ValueObject cuando sea business critical

### **✅ SIEMPRE HACER:**
1. **Seguir patterns existentes** → TipoServicio para categorías
2. **Respetar inheritance** → TenantEntityBase cuando aplicable  
3. **Private setters** → Rich domain model con behavior methods
4. **Backward compatibility** → Extensiones compatibles con código existente
5. **Domain consistency** → Mantener patterns establecidos

---

## 🎯 EXTENSIÓN ARTÍCULOS - DISEÑO CORRECTO

### **✅ APPROACH BASADO EN PATTERNS EXISTENTES:**

**Crear TipoArticulo (como TipoServicio):**
```csharp
public class TipoArticulo : EntityBase {
    public string Nombre { get; private set; } // "Shampoo", "Tratamiento", etc.
    // User-configurable categories, NO enum
}
```

**Crear Articulo (como Servicio):**
```csharp
public class Articulo : TenantEntityBase {
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public Dinero Precio { get; private set; } // ValueObject como Servicio
    public int Stock { get; private set; }
    public int TipoArticuloId { get; private set; } // FK dinámico
    public virtual TipoArticulo TipoArticulo { get; private set; }
}
```

**Extender VentaDetalle (NO breaking change):**
```csharp
public class VentaDetalle {
    // MANTENER propiedades existentes
    public int ServicioId { get; set; } // Existente
    
    // AGREGAR nuevas propiedades
    public int? ArticuloId { get; set; } // Nullable para backward compatibility
    public string TipoItem { get; set; } = "Servicio"; // "Servicio" o "Articulo"
    
    // Navigation
    public virtual Articulo? Articulo { get; set; }
}
```

---

## 🔧 WORKFLOW STATES - DISEÑO CORRECTO

### **✅ ESTADOS DINÁMICOS (NO ENUMS):**

**Crear EstadoWorkflow (como TipoServicio):**
```csharp
public class EstadoWorkflow : EntityBase {
    public string Nombre { get; private set; } // "Pendiente", "En Proceso", etc.
    public string Tipo { get; private set; } // "Servicio", "Venta", etc.
    public int Orden { get; private set; } // Para secuencia workflow
}
```

**Tracking en VentaDetalle:**
```csharp
public class VentaDetalle {
    // Workflow properties
    public int? EstadoWorkflowId { get; set; }
    public int? EmpleadoAsignadoId { get; set; } // Ya existe como EmpleadoServicioId
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaCompletado { get; set; }
    
    // Navigation
    public virtual EstadoWorkflow? EstadoWorkflow { get; set; }
}
```

---

## 💾 MIGRATION STRATEGY

### **🎯 EXTENSIÓN SIN BREAKING CHANGES:**
1. **Agregar tablas nuevas:** TipoArticulo, Articulo, EstadoWorkflow
2. **Extender VentaDetalle:** Nuevas columnas nullable + default values
3. **Mantener compatibilidad:** Código existente sigue funcionando
4. **Gradual adoption:** Nuevas features opcionales

### **🔄 BACKWARD COMPATIBILITY:**
- VentaDetalle.ServicioId sigue siendo requerido para ventas existentes
- TipoItem = "Servicio" default para registros existentes
- Nuevas propiedades nullable + migrations con defaults

---

## 🚨 IMPLEMENTACIÓN NEXT STEPS

### **📋 ORDEN IMPLEMENTACIÓN:**
1. **Crear entities:** TipoArticulo, Articulo, EstadoWorkflow
2. **Extender VentaDetalle:** Nullable additions + defaults
3. **Migration scripts:** ADD columns + default values
4. **Update DbContext:** New DbSets + configurations
5. **Services:** Business logic respetando patterns
6. **UI:** Workflow diferenciado empleado/cajero

**CRÍTICO: Este knowledge base debe ser consultado ANTES de cualquier cambio arquitectural futuro.**
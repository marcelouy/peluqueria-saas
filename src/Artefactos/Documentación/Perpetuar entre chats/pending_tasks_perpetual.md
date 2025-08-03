# 📋 LISTA PENDIENTES PERPETUAS - SISTEMA TRACKING

**📅 CREADO:** Julio 31, 2025  
**🎯 PROPÓSITO:** Perpetuar tareas críticas entre chats hasta completar  
**⚡ ESTADO:** ACTIVO - Evaluar cada chat + perpetuar hasta DONE  

---

## 🚨 ARQUITECTURALES CRÍTICAS - PRIORIDAD ALTA

### **🔴 MIXED IDs STANDARDIZATION (Perpetuar hasta resolver)**
- **ISSUE:** Sistema con int IDs (Ventas) + Guid IDs (Citas) mixed
- **IMPACT:** Complexity development + maintenance overhead
- **PLAN:** Document clearly + clean separation + future migration path defined
- **STATUS:** 🟡 ACCEPTED - needs documentation + boundaries 
- **CHAT RESPONSIBLE:** Evaluar cada chat si momento para standardizar
- **DONE CRITERIA:** Clear documentation + separation + migration plan OR standardización completa

### **🔴 FOREIGN KEY CASCADE CYCLES (Perpetuar hasta resolver)**
- **ISSUE:** EF migrations fallan por cascade delete cycles en entities existentes
- **IMPACT:** Cannot create clean database + deployment issues
- **PLAN:** OnDelete(DeleteBehavior.Restrict) para evitar cycles
- **STATUS:** 🟡 IN PROGRESS - DbContext being fixed
- **CHAT RESPONSIBLE:** Current chat fixing + future chats verify
- **DONE CRITERIA:** Migration exitosa + no cascade warnings + BD created clean

### **🔴 SHADOW PROPERTIES WARNINGS (Perpetuar hasta resolver)**
- **ISSUE:** EF creando `ClienteId1`, `EmpleadoId1` shadow properties por conflicts
- **IMPACT:** Performance + confusion + maintenance issues
- **PLAN:** Explicit FK configuration + proper naming
- **STATUS:** 🟡 IN PROGRESS - DbContext being fixed
- **CHAT RESPONSIBLE:** Current chat + verification next chats
- **DONE CRITERIA:** No shadow property warnings + explicit FK config + clean migration

---

## 🟡 IMPLEMENTACIÓN FEATURES - PRIORIDAD MEDIA

### **🟡 WORKFLOW EMPLOYEE vs CASHIER SEPARATION (Perpetuar hasta completar)**
- **ISSUE:** Empleados ven precios + totales (no deberían) vs Cajero control total
- **IMPACT:** Business requirement critical + operational security
- **PLAN:** Role-based UI + separate controllers + permission system
- **STATUS:** 🔵 PENDING - entities ready, UI needed
- **CHAT RESPONSIBLE:** Next chats after DB issues resolved
- **DONE CRITERIA:** Empleado UI (no prices) + Cajero UI (full) + role separation working

### **🟡 HYBRID PRINTING IMPLEMENTATION (Perpetuar hasta completar)**
- **ISSUE:** Auto-printing post-venta needs local/cloud compatibility
- **IMPACT:** User experience + competitive advantage + efficiency
- **PLAN:** Bridge service + web fallback + configuration UI
- **STATUS:** 🔵 PENDING - design done, implementation needed
- **CHAT RESPONSIBLE:** After core workflow completed
- **DONE CRITERIA:** Auto-print working + hybrid bridge/web + configuration UI

### **🟡 ARTICULOS STOCK MANAGEMENT (Perpetuar hasta completar)**
- **ISSUE:** Artículos entities created but no stock tracking logic
- **IMPACT:** Inventory management + business operations
- **PLAN:** Stock reduction on sale + low stock alerts + management UI
- **STATUS:** 🔵 PENDING - entities ready, business logic needed
- **CHAT RESPONSIBLE:** After workflow + printing completed
- **DONE CRITERIA:** Stock tracking + alerts + management UI + inventory reports

---

## 🟢 OPTIMIZACIÓN + MEJORAS - PRIORIDAD BAJA

### **🟢 DECIMAL PRECISION WARNINGS CLEANUP (Perpetuar hasta completar)**
- **ISSUE:** EF warnings `PrecioBase`, `Salario` no store type specified
- **IMPACT:** Data truncation potential + warning noise
- **PLAN:** HasColumnType("decimal(10,2)") explicit configuration
- **STATUS:** 🟡 IN PROGRESS - partially fixed in current DbContext
- **CHAT RESPONSIBLE:** Current chat + verification
- **DONE CRITERIA:** No decimal precision warnings + explicit configuration all entities

### **🟢 MULTI-TENANT CONSISTENCY (Perpetuar hasta completar)**
- **ISSUE:** Some entities TenantEntityBase, others simple entities inconsistent
- **IMPACT:** Future multi-tenant deployment + consistency
- **PLAN:** Standardize inheritance pattern + TenantId consistent
- **STATUS:** 🔵 PENDING - document pattern + gradual migration
- **CHAT RESPONSIBLE:** Future chats when architecture mature
- **DONE CRITERIA:** Consistent TenantEntityBase pattern + documentation + migration plan

### **🟢 PERFORMANCE OPTIMIZATION (Perpetuar hasta completar)**
- **ISSUE:** Missing indexes + query optimization + caching strategy
- **IMPACT:** Performance under load + scalability
- **PLAN:** Index analysis + query optimization + Redis caching
- **STATUS:** 🔵 PENDING - basic indexes added, advanced optimization needed
- **CHAT RESPONSIBLE:** Post-MVP when performance testing done
- **DONE CRITERIA:** Performance baseline + optimized queries + caching strategy + load testing

---

## 📋 INSTRUCCIONES PERPETUACIÓN

### **🚨 CADA CHAT DEBE:**
1. **LEER** esta lista completa al inicio
2. **EVALUAR** si momento adecuado tackle alguna tarea
3. **ACTUALIZAR** status si progreso realizado
4. **MARCAR DONE** si tarea completada totalmente
5. **PERPETUAR** lista actualizada en resumen_XXX.md
6. **AÑADIR** nuevas tareas críticas identificadas

### **✅ CRITERIOS PARA MARCAR DONE:**
- Task cumple **DONE CRITERIA** específico
- **Testing** confirma funcionalidad correcta
- **Documentation** updated si aplicable
- **No regressions** introduced
- **Chat responsible** confirms completion

### **🔄 ACTUALIZACIÓN FORMAT:**
```markdown
- **STATUS:** 🟢 DONE - COMPLETED by Chat X - Date
- **COMPLETED BY:** [Chat responsible + date]
- **VERIFICATION:** [How completion was verified]
```

---

## 🎯 PRIORIZACIÓN DINÁMICA

### **EVALUAR CADA CHAT:**
- **Red tasks** → Work immediately if blocking
- **Yellow tasks** → Work when dependencies resolved
- **Blue tasks** → Work when capacity available
- **Green tasks** → Work when system mature

### **DEPENDENCIES:**
1. **DB Issues** must resolve before features
2. **Workflow** before printing implementation
3. **Core features** before optimizations
4. **MVP** before advanced features

**Esta lista será perpetuada y actualizada en cada resumen_XXX.md hasta todas las tareas estén DONE.**
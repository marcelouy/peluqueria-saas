# 🛟 RECOVERY PLAN - MIGRACIÓN POS ERROR
**Fecha:** Julio 21, 2025  
**Situación:** PowerShell cerrado abruptamente durante migración  
**Objetivo:** Recovery seguro sin pérdida datos existentes  

---

## 🎯 ESCENARIOS POSIBLES Y RECOVERY

### **🟢 ESCENARIO 1: TODO OK (Mejor caso)**
**Síntomas:**
- ✅ Build exitoso
- ✅ App ejecuta sin errores  
- ✅ URLs /Empleados, /Servicios, /Clientes funcionan
- ✅ Tablas Ventas y VentaDetalles existen

**🎯 ACCIÓN:**
```
✅ SISTEMA PERFECTO - Continuar ITERACIÓN 2 (repositories)
No se requiere recovery - migración completada exitosamente
```

---

### **🟡 ESCENARIO 2: MIGRACIÓN PARCIAL (Común)**
**Síntomas:**
- ✅ Build exitoso
- ✅ App ejecuta sin errores
- ✅ URLs funcionan normalmente
- ❌ Tablas Ventas/VentaDetalles NO existen (consulta SQL retorna "NO EXISTE")

**🎯 RECOVERY SIMPLE:**
```powershell
# Re-ejecutar solo la migración EF
dotnet ef database update --project .\src\PeluqueriaSaaS.Infrastructure --startup-project .\src\PeluqueriaSaaS.Web
```

**✅ Verificación post-recovery:**
- Ejecutar consultas SQL verificar tablas creadas
- Confirmar URLs siguen funcionando

---

### **🟠 ESCENARIO 3: DBCONTEXT ROTO (Recuperable)**
**Síntomas:**
- ❌ Build ERROR (errores compilación)
- ❌ Mensajes sobre DbContext, DbSet, o entities
- ❌ App no ejecuta por errores compilación

**🎯 RECOVERY DBCONTEXT:**

#### **Opción A: Rollback parcial**
```
1. Abrir: src/PeluqueriaSaaS.Infrastructure/Data/PeluqueriaDbContext.cs
2. REMOVER líneas agregadas:
   - public DbSet<Venta> Ventas { get; set; }
   - public DbSet<VentaDetalle> VentaDetalles { get; set; }
   - Toda configuración OnModelCreating nuevas entities
3. Verificar build OK
4. Re-aplicar cambios cuidadosamente
```

#### **Opción B: Backup DbContext**
```
Si tienes backup del archivo original:
1. Restore PeluqueriaDbContext.cs desde backup
2. Verificar sistema funciona
3. Re-aplicar cambios paso a paso
```

---

### **🔴 ESCENARIO 4: BD CORRUPTA (Crítico)**
**Síntomas:**
- ❌ App no ejecuta (errores BD)
- ❌ Errores conexión o tablas inexistentes
- ❌ Sistema completamente no funcional

**🎯 RECOVERY COMPLETO:**
```sql
-- USAR backup_001.sql para restore completo
-- Este backup contiene estado funcional con:
-- - 12 empleados activos
-- - 10 servicios activos  
-- - 4 tipos servicio
-- - Todas las relaciones funcionando
```

**📝 Pasos recovery BD:**
1. Conectar SQL Server Management Studio
2. Drop database PeluqueriaSaaS (si existe)
3. Restore desde backup_001.sql
4. Verificar datos: 12 empleados, 10 servicios
5. Ejecutar app y confirmar URLs funcionan
6. Re-iniciar proceso migración desde cero

---

## 🚨 RECOVERY INMEDIATO REQUERIDO

### **🔍 PASO 1: EJECUTAR DIAGNÓSTICO**
```powershell
# Ejecutar comandos UNO POR UNO (no como script):
dotnet build .\src\PeluqueriaSaaS.Web
```

### **🔍 PASO 2: REPORTAR RESULTADO**
Indicar exactamente:
- ¿Build exitoso o error?
- Si error: ¿qué dice exactamente?
- ¿App ejecuta o falla?
- ¿URLs funcionan o dan error?

### **🔍 PASO 3: APLICAR RECOVERY APROPIADO**
Basado en escenario identificado, ejecutar recovery específico

---

## 🛡️ PREVENCIÓN FUTUROS ERRORES

### **✅ Lecciones aprendidas:**
- Scripts PS complejos pueden fallar abruptamente
- Ejecutar comandos críticos UNO POR UNO
- Siempre verificar estado ANTES de continuar
- Tener plan recovery ANTES de cambios

### **✅ Protocolo mejorado:**
- Comandos EF ejecutar manualmente
- Verificación inmediata post-cambio
- Backup states intermedios
- Never assume success

---

## 📞 COMUNICACIÓN REQUERIDA

**🎯 REPORTA EXACTAMENTE:**
1. Resultado `dotnet build`
2. Resultado `dotnet run` (si build OK)
3. Estado URLs (si app ejecuta)
4. Consultas SQL tablas (si app ejecuta)

**Con esta info determino escenario exacto y recovery apropiado.**

---

**🚨 NO PÁNICO:** Tenemos backup_001.sql completo, sistema es recuperable 100%. Solo necesito saber estado actual exacto.**
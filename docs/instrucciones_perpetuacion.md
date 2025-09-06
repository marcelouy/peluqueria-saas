# 📖 INSTRUCCIONES PARA PERPETUAR EL PROYECTO ENTRE CHATS

## 🎯 PROPÓSITO
Este documento garantiza la continuidad del desarrollo del sistema PeluqueriaSaaS entre diferentes sesiones de chat, preservando el conocimiento arquitectónico, decisiones técnicas y contexto del proyecto.

---

## 📋 AL INICIAR UN NUEVO CHAT

### Paso 1: Proporcionar Contexto
Comparte estos archivos en este orden:
1. `ARQUITECTURA_premisas_inmutables.md`
2. `RESUMEN_[NUMERO]_MAESTRO.md` (el más reciente)
3. `001_schema_completo_[FECHA].sql` (si hay cambios en BD)

### Paso 2: Mensaje de Inicio Tipo
```
Continuamos desarrollo del sistema PeluqueriaSaaS.
- Chat anterior: #[NUMERO]
- Estado: [XX]% funcional
- Objetivo hoy: [DESCRIPCIÓN]
- Leer resumen adjunto para contexto completo
```

---

## 📂 ESTRUCTURA DE DOCUMENTACIÓN

### Archivos Críticos a Mantener:

#### 1. **ARQUITECTURA_premisas_inmutables.md**
- Premisas que NUNCA cambian
- Reglas de oro del proyecto
- Valores del sistema
- Decisiones arquitectónicas fundamentales

#### 2. **RESUMEN_XXX_MAESTRO.md**
- Estado actual completo
- Historial de cambios
- Problemas resueltos/pendientes
- Próximos pasos
- Métricas del proyecto

#### 3. **Schema SQL**
- Backup del schema actual
- Incluir después de cambios importantes
- Nombrar: `001_schema_completo_YYYY-MM-DD.sql`

#### 4. **CHANGELOG_[MES]_[AÑO].md**
- Registro de cambios por versión
- Bugs resueltos
- Features agregadas
- Breaking changes

---

## 🔄 DURANTE EL DESARROLLO

### Mantener Actualizado:
1. **Después de cada cambio importante:**
   - Actualizar el resumen maestro
   - Documentar decisiones técnicas
   - Registrar problemas/soluciones

2. **Si modificas la BD:**
   - Documentar el SQL ejecutado
   - Actualizar schema si es estructural
   - Registrar en resumen

3. **Si agregas funcionalidad:**
   - Documentar archivos modificados
   - Explicar integración con existente
   - Actualizar métricas

---

## 📝 PLANTILLA RESUMEN MAESTRO

```markdown
# 📋 RESUMEN_[NUMERO]_MAESTRO - Sistema PeluqueriaSaaS
## [Título descriptivo del logro principal]

## 🎯 CONTEXTO DEL PROYECTO
- Información vital del proyecto
- Historial de resúmenes anteriores

## 🏗️ ARQUITECTURA Y PREMISAS FUNDAMENTALES
- Premisas MACRO (sistema)
- Premisas MICRO (código)
- Decisiones arquitectónicas

## 📊 ESTADO ACTUAL DEL SISTEMA
- Módulos funcionando
- Cambios recientes

## 🗂️ ESTRUCTURA DEL PROYECTO
- Árbol de directorios actualizado
- Archivos críticos marcados

## 💾 BASE DE DATOS
- Estado tablas
- Valores clave

## 🚀 PRÓXIMO DESARROLLO
- Prioridades ordenadas
- Tareas específicas

## 🔧 CONFIGURACIONES Y CONSTANTES
- Valores del sistema
- Búsquedas dinámicas

## 📝 COMANDOS ÚTILES
- Desarrollo
- SQL verificación

## 🐛 PROBLEMAS CONOCIDOS
- Resueltos ✅
- Pendientes ⚠️

## 📊 MÉTRICAS DEL PROYECTO
- Tabla con estadísticas

## 🚨 REGLAS DE ORO
- Lo que NUNCA hacer
- Lo que SIEMPRE hacer

## 💡 LECCIONES APRENDIDAS
- Técnicas exitosas
- Errores evitados

## 📚 INFORMACIÓN PARA PRÓXIMO CHAT
- Instrucciones continuidad
- Archivos críticos
```

---

## ⚠️ PREMISAS INMUTABLES - NUNCA VIOLAR

### Arquitectónicas:
1. **NO modificar** EntityBase.cs o TenantEntityBase.cs
2. **NO usar** Entity Framework Migrations
3. **NO hardcodear** IDs - usar búsquedas por clave natural
4. **NO cambiar** TenantId de "default"

### Operativas:
1. **SIEMPRE** documentar en resumen maestro
2. **SIEMPRE** hacer backup antes de cambios BD  
3. **SIEMPRE** buscar empleado sistema por email
4. **SIEMPRE** buscar cliente ocasional por nombre

### Técnicas:
1. Clean Architecture estricta
2. Repository Pattern
3. Domain-Driven Design
4. Entidades inmutables (private set)

---

## 🔍 BÚSQUEDAS Y VALORES CLAVE

```csharp
// Constantes del Sistema
DEFAULT_TENANT_ID = "default"
DEFAULT_SERIE = "A001"
EMPLEADO_SISTEMA_EMAIL = "sistema@peluqueria.com"

// Búsquedas Dinámicas
Cliente Ocasional: 
  WHERE Nombre = 'CLIENTE' AND Apellido = 'OCASIONAL'

Empleado Sistema:
  WHERE Email = 'sistema@peluqueria.com'

Settings Principal:
  WHERE CodigoPeluqueria = 'MAIN'
```

---

## 💻 STACK TECNOLÓGICO

### Backend:
- .NET 9
- C# 13
- Entity Framework Core 9
- SQL Server
- Clean Architecture

### Frontend:
- Razor Views
- jQuery
- Bootstrap 5
- Chart.js
- SweetAlert2 (pendiente)
- Sin frameworks SPA

### Herramientas:
- Visual Studio / VS Code
- SQL Server Management Studio
- Git (sin migrations en repo)

---

## 📈 CÓMO MEDIR PROGRESO

### Sistema Funcional %:
- 0-60%: Infraestructura base
- 60-80%: CRUD principales
- 80-90%: Funcionalidades core
- 90-95%: Integración y estabilización
- 95-99%: Reportes y optimización
- 100%: Producción ready

### Checklist Core:
- [ ] Empleados CRUD
- [ ] Clientes CRUD
- [ ] Servicios CRUD
- [ ] Artículos CRUD
- [ ] POS funcionando
- [ ] Comprobantes generándose
- [ ] Dashboard con métricas
- [ ] Reportes básicos
- [ ] Artículos en POS
- [ ] Exportación datos

---

## 🚀 WORKFLOW RECOMENDADO

### Para Bug Fix:
1. Identificar en qué módulo está
2. Revisar resumen para contexto
3. No violar premisas
4. Documentar solución

### Para Nueva Feature:
1. Verificar que no existe
2. Revisar arquitectura
3. Seguir patrones existentes
4. Actualizar resumen

### Para Cambios BD:
1. Backup primero
2. SQL manual (no migrations)
3. Documentar script
4. Actualizar schema

---

## 🔗 REFERENCIAS RÁPIDAS

### Archivos Críticos:
- `ComprobanteService.cs` - Lógica empleado sistema
- `EmpleadoRepository.cs` - GetByEmailAsync, CreateSistemaAsync
- `VentasController.cs` - POS sin duplicados
- `HomeController.cs` - Dashboard métricas

### Vistas Importantes:
- `Views/Ventas/POS.cshtml` - Punto de venta
- `Views/Home/Dashboard.cshtml` - Métricas
- `Views/Shared/_Layout.cshtml` - Layout principal

### JavaScript Principal:
- `wwwroot/js/pos.js` - Lógica POS
- `wwwroot/js/dashboard.js` - Charts
- `wwwroot/js/ventas-common.js` - Funciones compartidas

---

## 📌 TIPS PARA ÉXITO

1. **Lee TODO el resumen** antes de codear
2. **Respeta las premisas** aunque parezcan extrañas
3. **Documenta mientras desarrollas**, no después
4. **Usa búsquedas dinámicas**, no IDs fijos
5. **Prueba localmente** antes de confirmar
6. **Actualiza el resumen** al finalizar sesión
7. **Mantén el código consistente** con lo existente

---

## 🆘 ANTE PROBLEMAS

### Si algo no funciona:
1. Verificar no violaste premisas
2. Revisar último resumen funcionando
3. Verificar empleado sistema existe
4. Verificar cliente ocasional existe
5. Verificar TenantId = "default"

### Si necesitas rollback:
1. Los resúmenes tienen el estado en cada punto
2. El schema SQL está versionado
3. Git tiene los commits (sin migrations)

---

## 📮 MENSAJE TIPO PARA FINALIZAR CHAT

```
Chat #[NUMERO] completado.
- Logro principal: [DESCRIPCIÓN]
- Estado sistema: [XX]%
- Próximo objetivo: [DESCRIPCIÓN]
- Resumen actualizado: RESUMEN_[NUMERO]_MAESTRO.md

Archivos modificados:
- [Lista de archivos]

Para continuar:
- Leer RESUMEN_[NUMERO]_MAESTRO.md
- Foco en: [próxima tarea]
```

---

### **FIN INSTRUCCIONES_PERPETUACION_CHATS.md**

**Propósito:** Garantizar continuidad entre chats  
**Actualizar:** Cuando cambien procesos fundamentales  
**Compartir:** Al inicio de cada nuevo chat

---

*"La documentación es el puente entre el chat de hoy y el desarrollo de mañana"*
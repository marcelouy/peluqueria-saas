# 🎨 PREFERENCIAS PERSONALES DE ESTILOS DE BOTONES

## ESTILOS ESTÁNDAR DEFINIDOS
Usar SIEMPRE estos estilos en todas las futuras vistas:

### CLASES:
- btn-action btn-view (VER - Azul gradiente)
- btn-action btn-edit (EDITAR - Naranja gradiente)  
- btn-action btn-delete (ELIMINAR - Rojo gradiente)
- btn-action btn-create (CREAR - Verde gradiente)
- btn-action btn-back (VOLVER - Gris gradiente)

### CARACTERÍSTICAS:
- Gradientes hermosos con hover effects
- Tamaños consistentes (85px-120px width, 36-40px height)
- Sombras suaves con elevación en hover
- Bordes redondeados (8px)
- Transiciones suaves (0.3s ease)
- Override completo de Bootstrap

### ESTRUCTURA HTML:
```html
<div class="action-buttons">
    <a asp-action="Details" asp-route-id="@item.Id" class="btn btn-action btn-view">Ver</a>
    <a asp-action="Edit" asp-route-id="@item.Id" class="btn btn-action btn-edit">Editar</a>
    <a asp-action="Delete" asp-route-id="@item.Id" class="btn btn-action btn-delete">Eliminar</a>
</div>
```

### APLICAR EN:
- ✅ Empleados (aplicado)
- ✅ Clientes (aplicado)  
- 🔲 Servicios (pendiente)
- 🔲 Citas (pendiente)
- 🔲 Todas las futuras vistas

NOTA: Siempre incluir el CSS completo al inicio de cada vista para garantizar aplicación.

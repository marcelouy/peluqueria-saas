// SERVICIOS VALIDATION - Completo con todas funcionalidades
console.log('SERVICIOS VALIDATION - INICIO');

document.addEventListener('DOMContentLoaded', function() {
    console.log('SERVICIOS VALIDATION - DOM READY');
    
    // Configurar formulario principal
    const mainForm = document.querySelector('form:not([style*="display: none"])');
    if (mainForm) {
        mainForm.id = 'form';
    }
    
    // ===============================================
    // VALIDACION TIEMPO REAL
    // ===============================================
    
    // VALIDACION NOMBRE SERVICIO
    const nombreInput = document.getElementById('Nombre');
    if (nombreInput) {
        nombreInput.addEventListener('input', function() {
            const nombre = this.value.trim();
            const nombreFeedback = document.getElementById('nombre-feedback');
            
            // Limpiar clases previas
            this.classList.remove('is-valid', 'is-invalid');
            if (nombreFeedback) nombreFeedback.textContent = '';
            
            if (nombre.length > 0) {
                if (nombre.length >= 3) {
                    this.classList.add('is-valid');
                    if (nombreFeedback) {
                        nombreFeedback.textContent = 'Nombre valido';
                        nombreFeedback.className = 'text-success small';
                    }
                    console.log('Nombre valido:', nombre);
                } else {
                    this.classList.add('is-invalid');
                    if (nombreFeedback) {
                        nombreFeedback.textContent = 'Nombre debe tener al menos 3 caracteres';
                        nombreFeedback.className = 'text-danger small';
                    }
                    console.log('Nombre muy corto:', nombre);
                }
            }
        });
    }
    
    // VALIDACION PRECIO
    const precioInput = document.getElementById('Precio');
    if (precioInput) {
        precioInput.addEventListener('input', function() {
            const precio = parseFloat(this.value);
            const precioFeedback = document.getElementById('precio-feedback');
            
            // Limpiar clases previas
            this.classList.remove('is-valid', 'is-invalid');
            if (precioFeedback) precioFeedback.textContent = '';
            
            if (this.value.length > 0) {
                if (!isNaN(precio) && precio > 0) {
                    this.classList.add('is-valid');
                    if (precioFeedback) {
                        precioFeedback.textContent = 'Precio valido: $' + precio.toFixed(2);
                        precioFeedback.className = 'text-success small';
                    }
                    console.log('Precio valido:', precio);
                } else {
                    this.classList.add('is-invalid');
                    if (precioFeedback) {
                        precioFeedback.textContent = 'Precio debe ser un numero mayor a 0';
                        precioFeedback.className = 'text-danger small';
                    }
                    console.log('Precio invalido:', this.value);
                }
            }
        });
    }
    
    // VALIDACION DURACION
    const duracionInput = document.getElementById('DuracionMinutos');
    if (duracionInput) {
        duracionInput.addEventListener('input', function() {
            const duracion = parseInt(this.value);
            const duracionFeedback = document.getElementById('duracion-feedback');
            
            // Limpiar clases previas
            this.classList.remove('is-valid', 'is-invalid');
            if (duracionFeedback) duracionFeedback.textContent = '';
            
            if (this.value.length > 0) {
                if (!isNaN(duracion) && duracion > 0 && duracion <= 1440) {
                    this.classList.add('is-valid');
                    if (duracionFeedback) {
                        duracionFeedback.textContent = 'Duracion valida: ' + formatearDuracion(duracion);
                        duracionFeedback.className = 'text-success small';
                    }
                    console.log('Duracion valida:', duracion);
                } else {
                    this.classList.add('is-invalid');
                    if (duracionFeedback) {
                        duracionFeedback.textContent = 'Duracion debe ser entre 1 y 1440 minutos (24 horas)';
                        duracionFeedback.className = 'text-danger small';
                    }
                    console.log('Duracion invalida:', this.value);
                }
            }
        });
    }
    
    // VALIDACION DESCRIPCION
    const descripcionInput = document.getElementById('Descripcion');
    if (descripcionInput) {
        descripcionInput.addEventListener('input', function() {
            const descripcion = this.value.trim();
            const descripcionFeedback = document.getElementById('descripcion-feedback');
            
            // Limpiar clases previas
            this.classList.remove('is-valid', 'is-invalid');
            if (descripcionFeedback) descripcionFeedback.textContent = '';
            
            if (descripcion.length > 0) {
                if (descripcion.length <= 500) {
                    this.classList.add('is-valid');
                    if (descripcionFeedback) {
                        descripcionFeedback.textContent = 'Descripcion valida (' + descripcion.length + '/500)';
                        descripcionFeedback.className = 'text-success small';
                    }
                } else {
                    this.classList.add('is-invalid');
                    if (descripcionFeedback) {
                        descripcionFeedback.textContent = 'Descripcion muy larga (' + descripcion.length + '/500)';
                        descripcionFeedback.className = 'text-danger small';
                    }
                }
            }
        });
    }
    
    // ===============================================
    // VISTA PREVIA TIEMPO REAL
    // ===============================================
    
    function actualizarVistaPrevia() {
        const nombre = (nombreInput && nombreInput.value) || 'Nombre del servicio';
        const precio = (precioInput && precioInput.value) || '0.00';
        const monedaSelect = document.getElementById('MonedaCodigo');
        const moneda = (monedaSelect && monedaSelect.value) || 'UYU';
        const duracion = (duracionInput && duracionInput.value) || '0';
        const descripcion = (descripcionInput && descripcionInput.value) || 'Descripción del servicio aparecerá aquí...';
        const tipoSelect = document.getElementById('TipoServicioId');
        const tipo = tipoSelect && tipoSelect.options[tipoSelect.selectedIndex] ? tipoSelect.options[tipoSelect.selectedIndex].text : 'Tipo de servicio';
        
        // Actualizar elementos vista previa si existen
        const previewNombre = document.getElementById('preview-nombre');
        const previewTipo = document.getElementById('preview-tipo');
        const previewPrecio = document.getElementById('preview-precio');
        const previewDuracion = document.getElementById('preview-duracion');
        const previewDescripcion = document.getElementById('preview-descripcion');
        
        if (previewNombre) previewNombre.textContent = nombre;
        if (previewTipo) previewTipo.textContent = tipo !== '-- Seleccione un tipo --' ? tipo : 'Tipo de servicio';
        if (previewPrecio) previewPrecio.textContent = '$' + parseFloat(precio || 0).toFixed(2) + ' ' + moneda;
        if (previewDuracion) previewDuracion.textContent = formatearDuracion(parseInt(duracion || 0));
        if (previewDescripcion) previewDescripcion.textContent = descripcion;
    }
    
    // Event listeners para vista previa
    const campos = ['Nombre', 'Precio', 'MonedaCodigo', 'DuracionMinutos', 'Descripcion', 'TipoServicioId'];
    campos.forEach(campo => {
        const elemento = document.getElementById(campo);
        if (elemento) {
            elemento.addEventListener('input', actualizarVistaPrevia);
            elemento.addEventListener('change', actualizarVistaPrevia);
        }
    });
    
    // Actualizar vista previa inicial
    actualizarVistaPrevia();
    
    // ===============================================
    // AUTOCOMPLETADO NOMBRES
    // ===============================================
    
    const nombresComunes = [
        'Corte de cabello clásico',
        'Corte de cabello moderno', 
        'Peinado para evento',
        'Coloración completa',
        'Mechas',
        'Tratamiento hidratante',
        'Alisado permanente',
        'Ondas al agua',
        'Recorte de barba',
        'Afeitado clásico',
        'Lavado y peinado',
        'Permanente',
        'Extensiones',
        'Manicure',
        'Pedicure'
    ];
    
    if (nombreInput) {
        const datalist = document.createElement('datalist');
        datalist.id = 'nombres-sugeridos';
        nombresComunes.forEach(nombre => {
            const option = document.createElement('option');
            option.value = nombre;
            datalist.appendChild(option);
        });
        document.body.appendChild(datalist);
        nombreInput.setAttribute('list', 'nombres-sugeridos');
    }
    
    // ===============================================
    // VALIDACION FORMULARIO SUBMIT
    // ===============================================
    
    const form = document.getElementById('form');
    if (form) {
        form.addEventListener('submit', function(e) {
            let isValid = true;
            
            // Verificar nombre
            if (nombreInput && nombreInput.value.trim().length < 3) {
                nombreInput.classList.add('is-invalid');
                isValid = false;
            }
            
            // Verificar precio
            if (precioInput && precioInput.value) {
                const precio = parseFloat(precioInput.value);
                if (isNaN(precio) || precio <= 0) {
                    precioInput.classList.add('is-invalid');
                    isValid = false;
                }
            }
            
            // Verificar duracion
            if (duracionInput && duracionInput.value) {
                const duracion = parseInt(duracionInput.value);
                if (isNaN(duracion) || duracion <= 0 || duracion > 1440) {
                    duracionInput.classList.add('is-invalid');
                    isValid = false;
                }
            }
            
            // Verificar descripcion longitud
            if (descripcionInput && descripcionInput.value.length > 500) {
                descripcionInput.classList.add('is-invalid');
                isValid = false;
            }
            
            if (!isValid) {
                e.preventDefault();
                console.log('Formulario servicios tiene errores validacion');
            } else {
                console.log('Formulario servicios valido - submit permitido');
            }
        });
        
        // Validacion Bootstrap
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    }
    
    // ===============================================
    // FUNCIONES UTILIDAD
    // ===============================================
    
    // Función global para limpiar formulario (usada por botones)
    window.limpiarFormulario = function() {
        if (confirm('¿Estás seguro de que deseas limpiar todos los campos?')) {
            if (form) {
                form.reset();
                // Limpiar clases validacion
                const inputs = form.querySelectorAll('input, textarea, select');
                inputs.forEach(input => {
                    input.classList.remove('is-valid', 'is-invalid');
                });
                // Limpiar feedback
                const feedbacks = form.querySelectorAll('[id$="-feedback"]');
                feedbacks.forEach(feedback => {
                    feedback.textContent = '';
                    feedback.className = '';
                });
                actualizarVistaPrevia();
            }
        }
    };
    
    // Función global para guardar y crear otro
    window.guardarYCrearOtro = function() {
        if (form && form.checkValidity()) {
            // Copiar datos del formulario principal al formulario oculto
            const formData = new FormData(form);
            const hiddenForm = document.getElementById('crearOtroForm');
            
            if (hiddenForm) {
                // Limpiar formulario oculto
                hiddenForm.querySelectorAll('input:not([name="crearOtro"]):not([name="__RequestVerificationToken"])').forEach(input => input.remove());
                
                // Copiar datos
                for (let [key, value] of formData.entries()) {
                    if (key !== '__RequestVerificationToken') {
                        const input = document.createElement('input');
                        input.type = 'hidden';
                        input.name = key;
                        input.value = value;
                        hiddenForm.appendChild(input);
                    }
                }
                
                hiddenForm.submit();
            }
        } else if (form) {
            form.reportValidity();
        }
    };
    
    console.log('SERVICIOS VALIDATION - INICIALIZADO');
});

// ===============================================
// FUNCIONES UTILIDAD GLOBALES
// ===============================================

// Formatear duración
function formatearDuracion(minutos) {
    if (minutos === 0) return '0 minutos';
    if (minutos < 60) return minutos + ' min';
    if (minutos === 60) return '1 hora';
    if (minutos % 60 === 0) return (minutos / 60) + ' horas';
    
    const horas = Math.floor(minutos / 60);
    const mins = minutos % 60;
    return horas + 'h ' + mins + 'min';
}

console.log('SERVICIOS VALIDATION - FIN CARGA');
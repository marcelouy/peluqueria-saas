// ========================================
// VENTAS VALIDACIÓN Y PRESERVACIÓN DE DATOS
// Previene pérdida de trabajo en errores
// Ubicación: wwwroot/js/ventas-validacion.js
// ========================================

(function() {
    'use strict';
    
    // Namespace para evitar conflictos
    window.VentasValidacion = {
        
        // Key para sessionStorage
        STORAGE_KEY: 'ventaEnProgreso',
        
        // Inicializar validación y preservación
        init: function() {
            this.configurarValidacion();
            this.recuperarDatosGuardados();
            this.configurarAutoGuardado();
            this.configurarLimpiezaStorage();
        },
        
        // Configurar validación antes de submit
        configurarValidacion: function() {
            const form = document.getElementById('posForm');
            if (!form) return;
            
            form.addEventListener('submit', function(e) {
                // Validar campos requeridos
                const empleadoId = document.getElementById('VentaActual_EmpleadoId');
                const clienteId = document.getElementById('VentaActual_ClienteId');
                const serviciosCount = document.querySelectorAll('#serviciosVenta tr').length;
                
                let errores = [];
                
                // Validar empleado (obligatorio)
                if (!empleadoId || empleadoId.value === '' || empleadoId.value === '0') {
                    errores.push('• Debe seleccionar un empleado');
                    empleadoId?.classList.add('is-invalid');
                }
                
                // Validar servicios
                if (serviciosCount === 0) {
                    errores.push('• Debe agregar al menos un servicio');
                }
                
                // Si hay errores, prevenir submit y mostrar
                if (errores.length > 0) {
                    e.preventDefault();
                    
                    // Guardar estado actual antes de mostrar error
                    VentasValidacion.guardarEstado();
                    
                    // Mostrar errores con toast
                    if (window.toastr) {
                        toastr.warning(
                            'Por favor corrija lo siguiente:<br>' + errores.join('<br>'),
                            'Validación',
                            {
                                timeOut: 5000,
                                closeButton: true,
                                progressBar: true,
                                escapeHtml: false
                            }
                        );
                    } else {
                        alert('Por favor corrija:\n' + errores.join('\n'));
                    }
                    
                    return false;
                }
                
                // Si todo OK, limpiar storage al enviar
                sessionStorage.removeItem(this.STORAGE_KEY);
                return true;
            }.bind(this));
            
            // Quitar clase invalid al cambiar valor
            document.getElementById('VentaActual_EmpleadoId')?.addEventListener('change', function() {
                this.classList.remove('is-invalid');
            });
        },
        
        // Guardar estado actual en sessionStorage
        guardarEstado: function() {
            try {
                const estado = {
                    timestamp: new Date().toISOString(),
                    empleadoId: document.getElementById('VentaActual_EmpleadoId')?.value || '',
                    clienteId: document.getElementById('VentaActual_ClienteId')?.value || '',
                    fecha: document.getElementById('VentaActual_FechaVenta')?.value || '',
                    descuento: document.getElementById('descuentoInput')?.value || '0',
                    observaciones: document.getElementById('VentaActual_Observaciones')?.value || '',
                    servicios: this.obtenerServiciosActuales()
                };
                
                sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(estado));
                console.log('✅ Estado guardado en sessionStorage:', estado);
                
            } catch (error) {
                console.error('Error guardando estado:', error);
            }
        },
        
        // Obtener servicios actuales del DOM
        obtenerServiciosActuales: function() {
            const servicios = [];
            
            // Si existe la variable global serviciosEnVenta (del POS.cshtml)
            if (window.serviciosEnVenta && Array.isArray(window.serviciosEnVenta)) {
                return window.serviciosEnVenta;
            }
            
            // Fallback: leer del DOM
            document.querySelectorAll('#serviciosVenta tr').forEach(row => {
                const servicioData = {
                    servicioId: row.dataset.servicioId,
                    nombre: row.querySelector('td:first-child strong')?.textContent || '',
                    precio: parseFloat(row.querySelector('td:nth-child(2)')?.textContent.replace('$', '') || 0),
                    cantidad: parseInt(row.querySelector('.cantidad-input')?.value || 1)
                };
                
                if (servicioData.servicioId) {
                    servicios.push(servicioData);
                }
            });
            
            return servicios;
        },
        
        // Recuperar datos guardados
        recuperarDatosGuardados: function() {
            try {
                const estadoGuardado = sessionStorage.getItem(this.STORAGE_KEY);
                if (!estadoGuardado) return;
                
                const estado = JSON.parse(estadoGuardado);
                
                // Verificar que no sea muy viejo (máx 2 horas)
                const horasTranscurridas = (new Date() - new Date(estado.timestamp)) / (1000 * 60 * 60);
                if (horasTranscurridas > 2) {
                    sessionStorage.removeItem(this.STORAGE_KEY);
                    return;
                }
                
                // Mostrar mensaje de recuperación
                if (window.toastr) {
                    toastr.info(
                        'Se detectó una venta en progreso. Recuperando datos...',
                        'Recuperación automática',
                        { timeOut: 3000, progressBar: true }
                    );
                }
                
                // Restaurar campos básicos
                setTimeout(() => {
                    if (estado.empleadoId) {
                        const empleadoSelect = document.getElementById('VentaActual_EmpleadoId');
                        if (empleadoSelect) empleadoSelect.value = estado.empleadoId;
                    }
                    
                    if (estado.clienteId) {
                        const clienteSelect = document.getElementById('VentaActual_ClienteId');
                        if (clienteSelect) clienteSelect.value = estado.clienteId;
                    }
                    
                    if (estado.descuento) {
                        const descuentoInput = document.getElementById('descuentoInput');
                        if (descuentoInput) descuentoInput.value = estado.descuento;
                    }
                    
                    if (estado.observaciones) {
                        const obsTextarea = document.getElementById('VentaActual_Observaciones');
                        if (obsTextarea) obsTextarea.value = estado.observaciones;
                    }
                    
                    // Restaurar servicios si hay función global
                    if (estado.servicios && estado.servicios.length > 0 && window.restaurarServicios) {
                        window.restaurarServicios(estado.servicios);
                    }
                    
                    console.log('✅ Datos recuperados de sessionStorage');
                }, 500);
                
            } catch (error) {
                console.error('Error recuperando estado:', error);
                sessionStorage.removeItem(this.STORAGE_KEY);
            }
        },
        
        // Auto-guardar cada 30 segundos
        configurarAutoGuardado: function() {
            setInterval(() => {
                // Solo guardar si hay servicios agregados
                const hayServicios = document.querySelectorAll('#serviciosVenta tr').length > 0;
                if (hayServicios) {
                    this.guardarEstado();
                }
            }, 30000); // 30 segundos
        },
        
        // Limpiar storage al salir de la página (excepto submit)
        configurarLimpiezaStorage: function() {
            window.addEventListener('beforeunload', function(e) {
                // Si el form se está enviando, no hacer nada
                const formEnviandose = document.getElementById('posForm')?.dataset.submitting === 'true';
                if (formEnviandose) return;
                
                // Si hay servicios sin guardar, advertir
                const hayServicios = document.querySelectorAll('#serviciosVenta tr').length > 0;
                if (hayServicios) {
                    const mensaje = 'Hay una venta en progreso. ¿Está seguro de salir?';
                    e.returnValue = mensaje;
                    return mensaje;
                }
            });
            
            // Marcar form como enviándose al hacer submit
            document.getElementById('posForm')?.addEventListener('submit', function() {
                this.dataset.submitting = 'true';
            });
        }
    };
    
    // Inicializar cuando DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            VentasValidacion.init();
        });
    } else {
        VentasValidacion.init();
    }
    
})();

// Exponer función para que POS.cshtml pueda usarla
window.restaurarServicios = function(servicios) {
    if (!servicios || !Array.isArray(servicios)) return;
    
    console.log('Restaurando servicios:', servicios);
    
    // Trigger click en cada servicio para agregarlo
    servicios.forEach(servicio => {
        const boton = document.querySelector(`[data-servicio-id="${servicio.servicioId}"]`);
        if (boton) {
            // Simular clicks según cantidad
            for (let i = 0; i < (servicio.cantidad || 1); i++) {
                boton.click();
            }
        }
    });
};

// Exponer función para hacer serviciosEnVenta accesible globalmente
window.exponerServiciosEnVenta = function(servicios) {
    window.serviciosEnVenta = servicios;
};
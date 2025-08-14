// ========================================
// CONTROL DE FECHA DE VENTA POR ROL
// Solo administradores pueden cambiar fecha
// Ubicación: wwwroot/js/ventas-fecha-control.js
// ========================================

(function() {
    'use strict';
    
    window.VentasFechaControl = {
        
        // Verificar si es administrador (temporal con localStorage)
        esAdministrador: function() {
            // Por ahora usar localStorage, después se integrará con roles reales
            return localStorage.getItem('esAdministrador') === 'true';
        },
        
        // Inicializar control de fecha
        init: function() {
            const fechaInput = document.getElementById('VentaActual_FechaVenta');
            if (!fechaInput) return;
            
            const esAdmin = this.esAdministrador();
            
            if (!esAdmin) {
                // Si no es admin, bloquear fecha y poner hoy
                this.bloquearFecha(fechaInput);
            } else {
                // Si es admin, permitir cambio
                this.habilitarFecha(fechaInput);
            }
            
            // Agregar indicador visual
            this.agregarIndicadorVisual(fechaInput, esAdmin);
        },
        
        // Bloquear input de fecha
        bloquearFecha: function(input) {
            // Poner fecha de hoy
            const hoy = new Date().toISOString().split('T')[0];
            input.value = hoy;
            
            // Hacer readonly
            input.readOnly = true;
            input.style.backgroundColor = '#f8f9fa';
            input.style.cursor = 'not-allowed';
            
            // Agregar tooltip
            input.title = 'Solo administradores pueden cambiar la fecha';
            
            // Prevenir cambio
            input.addEventListener('click', function(e) {
                e.preventDefault();
                
                if (window.toastr) {
                    toastr.info('Solo administradores pueden cambiar la fecha de venta', 'Información', {
                        timeOut: 3000
                    });
                }
            });
            
            // Forzar fecha de hoy si intenta cambiar
            input.addEventListener('change', function() {
                const hoy = new Date().toISOString().split('T')[0];
                this.value = hoy;
            });
        },
        
        // Habilitar input de fecha
        habilitarFecha: function(input) {
            input.readOnly = false;
            input.style.backgroundColor = '';
            input.style.cursor = '';
            input.title = 'Puede cambiar la fecha de venta';
            
            // Agregar validación de rango (no futuro, máx 30 días atrás)
            input.addEventListener('change', function() {
                const fecha = new Date(this.value);
                const hoy = new Date();
                const hace30Dias = new Date();
                hace30Dias.setDate(hace30Dias.getDate() - 30);
                
                if (fecha > hoy) {
                    if (window.toastr) {
                        toastr.warning('No puede seleccionar una fecha futura', 'Validación');
                    }
                    this.value = hoy.toISOString().split('T')[0];
                } else if (fecha < hace30Dias) {
                    if (window.toastr) {
                        toastr.warning('No puede seleccionar una fecha mayor a 30 días atrás', 'Validación');
                    }
                    this.value = hoy.toISOString().split('T')[0];
                }
            });
        },
        
        // Agregar indicador visual de estado
        agregarIndicadorVisual: function(input, esAdmin) {
            // Buscar el contenedor del input
            const container = input.parentElement;
            if (!container) return;
            
            // Crear indicador
            const indicator = document.createElement('div');
            indicator.className = 'mt-1';
            
            if (esAdmin) {
                indicator.innerHTML = `
                    <small class="text-success">
                        <i class="fas fa-unlock"></i> 
                        Fecha editable (Administrador)
                    </small>
                `;
            } else {
                indicator.innerHTML = `
                    <small class="text-muted">
                        <i class="fas fa-lock"></i> 
                        Fecha bloqueada (Solo hoy)
                    </small>
                `;
            }
            
            // Insertar después del input
            input.parentNode.insertBefore(indicator, input.nextSibling);
        },
        
        // Toggle admin temporal (para testing)
        toggleAdmin: function() {
            const esAdmin = localStorage.getItem('esAdministrador') === 'true';
            localStorage.setItem('esAdministrador', !esAdmin);
            
            if (window.toastr) {
                toastr.info(
                    !esAdmin ? 'Modo administrador activado' : 'Modo cajero activado',
                    'Cambio de rol',
                    { 
                        timeOut: 3000,
                        onHidden: function() {
                            location.reload(); // Recargar para aplicar cambios
                        }
                    }
                );
            }
            
            return !esAdmin;
        }
    };
    
    // Inicializar cuando DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            VentasFechaControl.init();
        });
    } else {
        VentasFechaControl.init();
    }
    
})();

// Exponer función para cambiar rol (temporal para testing)
window.toggleAdminMode = function() {
    return VentasFechaControl.toggleAdmin();
};
// estaciones.js - Manejo de la vista de estaciones de trabajo

$(document).ready(function() {
    console.log('✅ Estaciones.js cargado');
    
    // Inicializar timers
    actualizarTimers();
    
    // Actualizar timers cada 30 segundos
    setInterval(actualizarTimers, 30000);
    
    // Auto-refresh cada 2 minutos
    setInterval(function() {
        console.log('Auto-refresh de la página');
        location.reload();
    }, 120000);
    
    // Manejar cambio de estado
    $('.estado-select').on('change', function() {
        const $select = $(this);
        const ventaDetalleId = $select.data('detalle-id');
        const nuevoEstadoId = $select.val();
        const estadoActual = $select.data('estado-actual');
        const empleadoId = $select.data('empleado-id');
        
        // Si no seleccionó nada, salir
        if (!nuevoEstadoId) {
            return;
        }
        
        // Obtener texto de la opción seleccionada
        const nuevoEstadoTexto = $select.find('option:selected').text();
        
        // Confirmar acción
        const mensaje = `¿Confirmar cambio de estado a "${nuevoEstadoTexto}"?`;
        if (!confirm(mensaje)) {
            $select.val(''); // Reset dropdown
            return;
        }
        
        // Deshabilitar select mientras procesa
        $select.prop('disabled', true);
        
        // Mostrar indicador de carga
        const $card = $select.closest('.card');
        $card.addClass('opacity-50');
        
        // Hacer petición AJAX
        $.ajax({
            url: '/Estaciones/ActualizarEstado',
            type: 'POST',
            data: {
                ventaDetalleId: ventaDetalleId,
                nuevoEstadoId: parseInt(nuevoEstadoId),
                empleadoId: empleadoId || null
            },
            success: function(response) {
                if (response.success) {
                    console.log('✅ Estado actualizado:', response.message);
                    
                    // Mostrar notificación de éxito
                    mostrarNotificacion('success', response.message);
                    
                    // Si el estado es Completado o Cancelado, ocultar la tarjeta
                    if (nuevoEstadoId === '3' || nuevoEstadoId === '4') {
                        $card.fadeOut(500, function() {
                            $(this).remove();
                            verificarServiciosVacios();
                        });
                    } else {
                        // Recargar página para ver cambios
                        setTimeout(function() {
                            location.reload();
                        }, 1000);
                    }
                } else {
                    console.error('❌ Error:', response.message);
                    mostrarNotificacion('error', response.message || 'Error al actualizar estado');
                    
                    // Restaurar select
                    $select.val('').prop('disabled', false);
                    $card.removeClass('opacity-50');
                }
            },
            error: function(xhr, status, error) {
                console.error('❌ Error AJAX:', error);
                mostrarNotificacion('error', 'Error de conexión al actualizar estado');
                
                // Restaurar select
                $select.val('').prop('disabled', false);
                $card.removeClass('opacity-50');
            }
        });
    });
    
    // Función para actualizar todos los timers
    function actualizarTimers() {
        $('.timer-display').each(function() {
            const $timer = $(this);
            const inicioStr = $timer.data('inicio');
            
            if (!inicioStr) return;
            
            const inicio = new Date(inicioStr);
            const ahora = new Date();
            const diffMs = ahora - inicio;
            const diffMins = Math.floor(diffMs / 60000);
            
            let textoTiempo;
            if (diffMins < 1) {
                textoTiempo = 'Recién iniciado';
            } else if (diffMins < 60) {
                textoTiempo = diffMins + ' min';
            } else {
                const horas = Math.floor(diffMins / 60);
                const mins = diffMins % 60;
                textoTiempo = horas + 'h ' + mins + 'min';
            }
            
            $timer.find('.timer-text').text(textoTiempo);
            
            // Cambiar color según el tiempo transcurrido
            if (diffMins > 120) {
                $timer.addClass('text-danger'); // Más de 2 horas
            } else if (diffMins > 60) {
                $timer.addClass('text-warning'); // Más de 1 hora
            } else {
                $timer.addClass('text-success');
            }
        });
    }
    
    // Función para verificar si no hay servicios
    function verificarServiciosVacios() {
        if ($('.servicio-card').length === 0) {
            location.reload(); // Recargar para mostrar mensaje de "no hay servicios"
        }
    }
    
    // Función para mostrar notificaciones
    function mostrarNotificacion(tipo, mensaje) {
        // Remover notificaciones anteriores
        $('.notificacion-flotante').remove();
        
        const claseAlerta = tipo === 'success' ? 'alert-success' : 'alert-danger';
        const icono = tipo === 'success' ? 'check-circle' : 'exclamation-circle';
        
        const $notificacion = $(`
            <div class="notificacion-flotante alert ${claseAlerta} alert-dismissible fade show" role="alert">
                <i class="fas fa-${icono}"></i> ${mensaje}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `);
        
        $('body').append($notificacion);
        
        // Auto-cerrar después de 3 segundos
        setTimeout(function() {
            $notificacion.fadeOut(function() {
                $(this).remove();
            });
        }, 3000);
    }
    
    // Resaltar tarjetas en proceso
    $('.servicio-card').each(function() {
        const $card = $(this);
        const estadoActual = $card.find('.estado-select').data('estado-actual');
        
        if (estadoActual === 2) { // En Proceso
            $card.find('.card').addClass('shadow-lg');
        }
    });
    
    // Botón de actualizar manual
    $('#btn-refresh').on('click', function() {
        location.reload();
    });
    
    // Keyboard shortcuts
    $(document).on('keydown', function(e) {
        // F5 o Ctrl+R para refrescar
        if (e.key === 'F5' || (e.ctrlKey && e.key === 'r')) {
            e.preventDefault();
            location.reload();
        }
    });
});

// Función global para obtener estado de servicio
window.obtenerEstadoServicio = function(ventaDetalleId) {
    return $.ajax({
        url: '/Estaciones/ObtenerEstadoServicio',
        type: 'GET',
        data: { ventaDetalleId: ventaDetalleId }
    });
};
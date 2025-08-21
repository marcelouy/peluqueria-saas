// estaciones.js - Manejo de la vista de estaciones de trabajo con auto-refresh

$(document).ready(function() {
    console.log('✅ Estaciones.js cargado');
    
    // Variables globales
    let refreshInterval;
    const REFRESH_TIME = 30000; // 30 segundos
    
    // Inicializar
    inicializar();
    
    function inicializar() {
        // Inicializar timers
        actualizarTimers();
        
        // Actualizar timers cada segundo
        setInterval(actualizarTimers, 1000);
        
        // Auto-refresh cada 30 segundos - COMENTADO TEMPORALMENTE PARA PRUEBAS
        // refreshInterval = setInterval(function() {
        //     console.log('🔄 Auto-refresh estaciones...');
        //     location.reload();
        // }, REFRESH_TIME);
        
        // Mostrar countdown
        mostrarCountdown();
    }
    
    // Mostrar countdown para próximo refresh
    function mostrarCountdown() {
        let segundosRestantes = REFRESH_TIME / 1000;
        
        if ($('#countdown-container').length === 0) {
            $('body').append(`
                <div id="countdown-container" style="position: fixed; top: 10px; right: 10px; z-index: 1000;">
                    <small class="badge bg-info">
                        Actualización en: <span id="countdown-seconds">${segundosRestantes}</span>s
                    </small>
                </div>
            `);
        }
        
        setInterval(function() {
            segundosRestantes--;
            if (segundosRestantes <= 0) {
                segundosRestantes = REFRESH_TIME / 1000;
            }
            $('#countdown-seconds').text(segundosRestantes);
        }, 1000);
    }
    
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

// ========== FUNCIONES GLOBALES FUERA DEL DOCUMENT.READY ==========

// Función para mostrar notificaciones (movida afuera para ser global)
function mostrarNotificacion(tipo, mensaje) {
    // Remover notificaciones anteriores
    $('.notificacion-flotante').remove();
    
    const claseAlerta = tipo === 'success' ? 'alert-success' : 'alert-danger';
    const icono = tipo === 'success' ? 'check-circle' : 'exclamation-circle';
    
    const $notificacion = $(`
        <div class="notificacion-flotante alert ${claseAlerta} alert-dismissible fade show" 
             style="position: fixed; top: 20px; right: 20px; z-index: 9999;" role="alert">
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

// Función para preparar el cambio de empleado
function prepararCambioEmpleado(ventaDetalleId, empleadoActual, servicioNombre) {
    console.log('📍 Preparando cambio - ID:', ventaDetalleId, 'Empleado:', empleadoActual);
    $('#ventaDetalleIdModal').val(ventaDetalleId);
    $('#empleadoActual').text(empleadoActual);
    $('#servicioNombre').text(servicioNombre);
    $('#nuevoEmpleadoSelect').val('');
}

// Función para confirmar el cambio de empleado
function confirmarCambioEmpleado() {
    const ventaDetalleId = $('#ventaDetalleIdModal').val();
    const nuevoEmpleadoId = $('#nuevoEmpleadoSelect').val();
    
    console.log('📍 Confirmando cambio - VentaDetalle:', ventaDetalleId, 'Nuevo Empleado:', nuevoEmpleadoId);
    
    if (!nuevoEmpleadoId) {
        alert('Por favor selecciona un empleado');
        return;
    }
    
    // Deshabilitar botón mientras procesa
    const $btn = $('#cambiarEmpleadoModal .btn-primary');
    $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Cambiando...');
    
    // Hacer petición AJAX para cambiar empleado
    $.ajax({
        url: '/Estaciones/CambiarEmpleado',
        type: 'POST',
        data: {
            ventaDetalleId: ventaDetalleId,
            nuevoEmpleadoId: nuevoEmpleadoId
        },
        success: function(response) {
            console.log('📍 Respuesta:', response);
            if (response.success) {
                // Cerrar modal
                $('#cambiarEmpleadoModal').modal('hide');
                
                // Mostrar notificación de éxito
                mostrarNotificacion('success', 'Empleado cambiado exitosamente');
                
                // Recargar página después de un breve delay
                setTimeout(function() {
                    location.reload();
                }, 1000);
            } else {
                alert('❌ Error: ' + response.message);
                // Restaurar botón
                $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Confirmar Cambio');
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error AJAX:', error);
            alert('❌ Error al cambiar empleado: ' + error);
            // Restaurar botón
            $btn.prop('disabled', false).html('<i class="fas fa-save"></i> Confirmar Cambio');
        }
    });
}

// Función para obtener estado de servicio
function obtenerEstadoServicio(ventaDetalleId) {
    return $.ajax({
        url: '/Estaciones/ObtenerEstadoServicio',
        type: 'GET',
        data: { ventaDetalleId: ventaDetalleId }
    });
}
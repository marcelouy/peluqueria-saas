// monitor.js - Sistema de monitoreo en tiempo real con auto-refresh

$(document).ready(function() {
    console.log('✅ Monitor.js iniciado');
    
    // Variables globales
    let updateInterval;
    const REFRESH_INTERVAL = 60000; // 60 segundos = 1 minuto
    
    // Inicializar
    inicializarMonitor();
    
    function inicializarMonitor() {
        // Iniciar reloj digital
        actualizarReloj();
        setInterval(actualizarReloj, 1000);
        
        // Iniciar timers
        actualizarTimers();
        setInterval(actualizarTimers, 1000);
        
        // Actualizar estadísticas
        actualizarEstadisticas();
        
        // AUTO-REFRESH AUTOMÁTICO CADA 10 SEGUNDOS
        updateInterval = setInterval(function() {
            console.log('🔄 Auto-refresh automático...');
            location.reload(); // Recargar página completa
        }, REFRESH_INTERVAL);
        
        // Mostrar countdown para próximo refresh
        mostrarCountdown();
    }
    
    // Mostrar countdown para próximo refresh
    function mostrarCountdown() {
        let segundosRestantes = REFRESH_INTERVAL / 1000;
        
        // Crear elemento de countdown si no existe
        if ($('#countdown-refresh').length === 0) {
            $('.position-fixed').prepend(`
                <div id="countdown-refresh" class="mb-2 text-center">
                    <small class="text-muted">Actualización en: <span id="segundos-restantes">${segundosRestantes}</span>s</small>
                </div>
            `);
        }
        
        // Actualizar countdown cada segundo
        setInterval(function() {
            segundosRestantes--;
            if (segundosRestantes <= 0) {
                segundosRestantes = REFRESH_INTERVAL / 1000;
            }
            $('#segundos-restantes').text(segundosRestantes);
        }, 1000);
    }
    
    // Actualizar reloj digital
    function actualizarReloj() {
        const ahora = new Date();
        const horas = String(ahora.getHours()).padStart(2, '0');
        const minutos = String(ahora.getMinutes()).padStart(2, '0');
        const segundos = String(ahora.getSeconds()).padStart(2, '0');
        $('#reloj-digital').text(`${horas}:${minutos}:${segundos}`);
    }
    
    // Actualizar timers
    function actualizarTimers() {
        $('.timer-live').each(function() {
            const $timer = $(this);
            const inicioStr = $timer.data('inicio');
            
            if (!inicioStr) return;
            
            const inicio = new Date(inicioStr);
            const ahora = new Date();
            const diffMs = ahora - inicio;
            const totalSeconds = Math.floor(diffMs / 1000);
            const minutes = Math.floor(totalSeconds / 60);
            const seconds = totalSeconds % 60;
            
            $timer.text(`${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`);
            
            // Cambiar color según tiempo transcurrido
            if (minutes > 60) {
                $timer.parent().removeClass('bg-primary bg-warning').addClass('bg-danger');
            } else if (minutes > 30) {
                $timer.parent().removeClass('bg-primary bg-danger').addClass('bg-warning');
            }
        });
    }
    
    // Actualizar estadísticas
    function actualizarEstadisticas() {
        const enProceso = $('#panel-en-proceso .servicio-card').length;
        const esperando = $('#panel-esperando .servicio-card').length;
        
        $('#total-en-proceso').text(enProceso);
        $('#total-esperando').text(esperando);
    }
    
    // Permitir pausar/reanudar auto-refresh
    window.toggleAutoRefresh = function() {
        if (updateInterval) {
            clearInterval(updateInterval);
            updateInterval = null;
            $('#btn-toggle-refresh').html('<i class="fas fa-play"></i> Reanudar');
            $('#countdown-refresh').hide();
            console.log('⏸️ Auto-refresh pausado');
        } else {
            updateInterval = setInterval(function() {
                location.reload();
            }, REFRESH_INTERVAL);
            $('#btn-toggle-refresh').html('<i class="fas fa-pause"></i> Pausar');
            $('#countdown-refresh').show();
            console.log('▶️ Auto-refresh reanudado');
        }
    };
});
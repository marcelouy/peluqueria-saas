// ========================================
// CONFIGURACIÓN GLOBAL DE TOASTR
// Notificaciones toast para feedback usuario
// Ubicación: wwwroot/js/toastr-config.js
// ========================================

(function() {
    'use strict';
    
    // Configuración global de toastr
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            closeButton: true,
            debug: false,
            newestOnTop: true,
            progressBar: true,
            positionClass: "toast-top-right",
            preventDuplicates: true,
            onclick: null,
            showDuration: "300",
            hideDuration: "1000",
            timeOut: "5000",
            extendedTimeOut: "1000",
            showEasing: "swing",
            hideEasing: "linear",
            showMethod: "fadeIn",
            hideMethod: "fadeOut"
        };
    }
    
    // Helper para mostrar mensajes de TempData
    window.MostrarMensajesTempData = function() {
        // Buscar elementos con data-tempdata
        const successMsg = document.querySelector('[data-tempdata-success]');
        const errorMsg = document.querySelector('[data-tempdata-error]');
        const warningMsg = document.querySelector('[data-tempdata-warning]');
        const infoMsg = document.querySelector('[data-tempdata-info]');
        
        if (successMsg && successMsg.dataset.tempdataSuccess) {
            // Para ventas exitosas, mostrar con más tiempo y sonido
            if (successMsg.dataset.tempdataSuccess.includes('Venta #')) {
                toastr.success(successMsg.dataset.tempdataSuccess, '✅ Éxito', {
                    timeOut: 7000,
                    progressBar: true,
                    closeButton: true,
                    escapeHtml: false,
                    onShown: function() {
                        // Sonido de caja registradora (opcional)
                        reproducirSonidoExito();
                    }
                });
                
                // Agregar confeti o animación (opcional)
                if (window.confetti) {
                    confetti({
                        particleCount: 100,
                        spread: 70,
                        origin: { y: 0.6 }
                    });
                }
            } else {
                toastr.success(successMsg.dataset.tempdataSuccess, 'Éxito');
            }
        }
        
        if (errorMsg && errorMsg.dataset.tempdataError) {
            toastr.error(errorMsg.dataset.tempdataError, 'Error', {
                timeOut: 8000,
                closeButton: true
            });
        }
        
        if (warningMsg && warningMsg.dataset.tempdataWarning) {
            toastr.warning(warningMsg.dataset.tempdataWarning, 'Atención', {
                timeOut: 6000,
                closeButton: true,
                escapeHtml: false
            });
        }
        
        if (infoMsg && infoMsg.dataset.tempdataInfo) {
            toastr.info(infoMsg.dataset.tempdataInfo, 'Información', {
                timeOut: 4000
            });
        }
    };
    
    // Función para reproducir sonido de éxito (opcional)
    window.reproducirSonidoExito = function() {
        try {
            // Solo si el usuario ha configurado sonidos
            const sonidosHabilitados = localStorage.getItem('sonidosHabilitados') === 'true';
            if (!sonidosHabilitados) return;
            
            // Crear audio elemento temporal (sonido cash register)
            const audio = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmwhBDGEzvLTgjMGHm7A7+OZURE');
            audio.volume = 0.3;
            audio.play().catch(() => {
                // Ignorar errores de reproducción
            });
        } catch (e) {
            // Ignorar errores
        }
    };
    
    // Función para habilitar/deshabilitar sonidos
    window.toggleSonidos = function() {
        const actual = localStorage.getItem('sonidosHabilitados') === 'true';
        localStorage.setItem('sonidosHabilitados', !actual);
        
        if (!actual) {
            toastr.info('Sonidos habilitados', 'Configuración');
            reproducirSonidoExito();
        } else {
            toastr.info('Sonidos deshabilitados', 'Configuración');
        }
        
        return !actual;
    };
    
    // Inicializar al cargar DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            MostrarMensajesTempData();
        });
    } else {
        MostrarMensajesTempData();
    }
    
})();
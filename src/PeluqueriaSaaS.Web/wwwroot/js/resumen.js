// AGREGAR A: wwwroot/js/ventas.js O wwwroot/js/resumen.js
// FUNCIÓN JAVASCRIPT PARA DESCARGAR PDF

// ✅ FUNCIÓN DESCARGAR RESUMEN PDF
function descargarResumenPDF() {
    console.log('🎯 Iniciando descarga PDF resumen');
    
    // ✅ OBTENER VENTA ID DEL MODAL O CONTEXT
    const ventaId = window.currentVentaId || getVentaIdFromModal() || getVentaIdFromURL();
    
    if (!ventaId) {
        console.error('❌ VentaId no disponible para PDF');
        alert('Error: No se pudo obtener el ID de la venta para generar el PDF');
        return;
    }
    
    console.log(`🎯 Descargando PDF para VentaId: ${ventaId}`);
    
    try {
        // ✅ CREAR FORM PARA POST REQUEST
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Settings/ExportarPDF';
        form.target = '_blank'; // Abrir en nueva ventana si es necesario
        
        // ✅ AGREGAR VENTA ID COMO HIDDEN INPUT
        const ventaInput = document.createElement('input');
        ventaInput.type = 'hidden';
        ventaInput.name = 'ventaId';
        ventaInput.value = ventaId;
        form.appendChild(ventaInput);
        
        // ✅ AGREGAR ANTI-FORGERY TOKEN SI EXISTE
        const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]');
        if (antiForgeryToken) {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = antiForgeryToken.value;
            form.appendChild(tokenInput);
        }
        
        // ✅ AGREGAR FORM AL DOM Y SUBMIT
        document.body.appendChild(form);
        form.submit();
        
        // ✅ LIMPIAR FORM DESPUÉS DE SUBMIT
        setTimeout(() => {
            document.body.removeChild(form);
        }, 100);
        
        console.log('✅ Solicitud PDF enviada correctamente');
        
    } catch (error) {
        console.error('❌ Error descargando PDF:', error);
        alert('Error al descargar el PDF. Por favor, inténtelo nuevamente.');
    }
}

// ✅ HELPER FUNCTIONS PARA OBTENER VENTA ID
function getVentaIdFromModal() {
    // Buscar en el modal actual
    const modalElement = document.querySelector('#resumenModal');
    if (modalElement) {
        // Buscar en data attributes
        const ventaId = modalElement.getAttribute('data-venta-id');
        if (ventaId) return ventaId;
        
        // Buscar en el título del modal
        const modalTitle = modalElement.querySelector('.modal-title');
        if (modalTitle && modalTitle.textContent) {
            const match = modalTitle.textContent.match(/Venta #(\d+)/);
            if (match) return match[1];
        }
    }
    
    return null;
}

function getVentaIdFromURL() {
    // Buscar en la URL si estamos en página de detalle
    const urlMatch = window.location.pathname.match(/\/Ventas\/Details\/(\d+)/);
    if (urlMatch) return urlMatch[1];
    
    // Buscar en parámetros URL
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get('ventaId') || urlParams.get('id');
}

// ✅ FUNCIÓN PARA ESTABLECER VENTA ID GLOBAL (LLAMAR DESDE GENERAR RESUMEN)
function setCurrentVentaId(ventaId) {
    window.currentVentaId = ventaId;
    console.log(`🎯 VentaId establecido globalmente: ${ventaId}`);
}
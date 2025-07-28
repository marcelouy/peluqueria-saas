/*
 * Archivo: wwwroot/js/clientes-export.js
 * Propósito: Export Excel clientes functionality
 * Patrón: Similar a servicios-index.js export
 */

console.log('📊 clientes-export.js cargado correctamente');

document.addEventListener('DOMContentLoaded', function() {
    console.log('👥 clientes-export.js: DOM ready - inicializando export clientes');
    
    // Initialize export functionality
    initializeClientesExport();
    
    console.log('✅ clientes-export.js: Export clientes inicializado');
});

// ============================================================================
// EXPORT EXCEL CLIENTES FUNCTIONALITY
// ============================================================================

function initializeClientesExport() {
    console.log('📊 Inicializando export Excel clientes');
    
    const exportButton = document.getElementById('export-clientes-excel-btn');
    
    if (exportButton) {
        exportButton.addEventListener('click', function() {
            console.log('📊 Iniciando export Excel clientes');
            exportClientesToExcel();
        });
        
        console.log('✅ Botón export clientes Excel configurado');
    } else {
        console.warn('⚠️ Botón export clientes no encontrado');
    }
}

function exportClientesToExcel() {
    try {
        console.log('📊 Exportando clientes a Excel');
        
        // Mostrar loading en botón
        const exportButton = document.getElementById('export-clientes-excel-btn');
        const originalText = exportButton.innerHTML;
        exportButton.disabled = true;
        exportButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Generando...';
        
        // URL export simple (sin filtros por ahora)
        const exportUrl = '/Clientes/ExportarClientesExcel';
        
        console.log('📊 URL export clientes:', exportUrl);
        
        // Descargar archivo
        window.location.href = exportUrl;
        
        // Mostrar mensaje éxito
        showFeedbackClientes('success', 'Excel de clientes generado correctamente. La descarga comenzará automáticamente.');
        
        // Restaurar botón después de delay
        setTimeout(() => {
            exportButton.disabled = false;
            exportButton.innerHTML = originalText;
        }, 3000);
        
    } catch (error) {
        console.error('❌ Error exportando clientes Excel:', error);
        showFeedbackClientes('error', 'Error al generar el archivo Excel de clientes. Intenta nuevamente.');
        
        // Restaurar botón
        const exportButton = document.getElementById('export-clientes-excel-btn');
        exportButton.disabled = false;
        exportButton.innerHTML = '<i class="fas fa-file-excel"></i> Exportar Excel';
    }
}

// ============================================================================
// FEEDBACK SYSTEM
// ============================================================================

function showFeedbackClientes(type, message) {
    console.log(`💬 Mostrando feedback clientes ${type}: ${message}`);
    
    // Remove existing feedback
    const existingFeedback = document.querySelector('.feedback-clientes-alert');
    if (existingFeedback) {
        existingFeedback.remove();
    }
    
    // Create new feedback
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const iconClass = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle';
    
    const feedbackHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show feedback-clientes-alert" role="alert">
            <i class="fas ${iconClass}"></i> ${message}
            <button type="button" class="close" data-dismiss="alert">
                <span>&times;</span>
            </button>
        </div>
    `;
    
    // Insert after page header
    const pageHeader = document.querySelector('.page-header');
    if (pageHeader) {
        pageHeader.insertAdjacentHTML('afterend', feedbackHtml);
    } else {
        // Fallback: insert at top of main content
        const mainContent = document.querySelector('.container-fluid');
        if (mainContent) {
            mainContent.insertAdjacentHTML('afterbegin', feedbackHtml);
        }
    }
    
    // Auto-hide after 5 seconds
    setTimeout(() => {
        const feedbackElement = document.querySelector('.feedback-clientes-alert');
        if (feedbackElement) {
            feedbackElement.classList.remove('show');
            setTimeout(() => feedbackElement.remove(), 300);
        }
    }, 5000);
}

console.log('📊 clientes-export.js: Todas las funciones definidas correctamente');
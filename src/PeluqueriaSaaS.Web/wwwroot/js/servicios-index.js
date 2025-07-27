/*
 * Archivo: wwwroot/js/servicios-index.js
 * Propósito: JavaScript para Index Servicios - Filtros + AJAX cambiar estado
 * Fecha: Julio 2025
 * Patrón: JavaScript separado + AJAX + feedback + debugging
 */

console.log('🚀 servicios-index.js cargado correctamente');

document.addEventListener('DOMContentLoaded', function() {
    console.log('📋 servicios-index.js: DOM ready - inicializando funcionalidades');
    
    // Initialize all functionality
    initializeFilters();
    initializeStateToggle();
    initializeStatistics();
    initializeExport();
    
    console.log('✅ servicios-index.js: Todas las funcionalidades inicializadas');
});

// ============================================================================
// FILTROS FUNCTIONALITY
// ============================================================================

function initializeFilters() {
    console.log('🔍 Inicializando filtros de servicios');
    
    const filterForm = document.getElementById('filtros-form');
    const clearFiltersBtn = document.getElementById('clear-filters-btn');
    
    if (filterForm) {
        // Auto-submit on filter change
        const filterInputs = filterForm.querySelectorAll('input, select');
        filterInputs.forEach(input => {
            input.addEventListener('change', function() {
                console.log(`🔄 Filtro ${input.name} cambiado a: ${input.value}`);
                filterForm.submit();
            });
        });
        
        console.log('✅ Filtros configurados con auto-submit');
    }
    
    if (clearFiltersBtn) {
        clearFiltersBtn.addEventListener('click', function() {
            console.log('🗑️ Limpiando filtros');
            window.location.href = '/Servicios';
        });
        
        console.log('✅ Botón limpiar filtros configurado');
    }
}

// ============================================================================
// CAMBIAR ESTADO FUNCTIONALITY
// ============================================================================

function initializeStateToggle() {
    console.log('🔄 Inicializando cambio de estado AJAX');
    
    const toggleButtons = document.querySelectorAll('.toggle-estado-btn');
    
    toggleButtons.forEach(button => {
        button.addEventListener('click', function() {
            const servicioId = this.dataset.servicioId;
            const estadoActual = this.dataset.estadoActual === 'true';
            const nuevoEstado = !estadoActual;
            
            console.log(`🔄 Cambiando estado servicio ${servicioId}: ${estadoActual} → ${nuevoEstado}`);
            
            toggleEstadoServicio(servicioId, nuevoEstado, this);
        });
    });
    
    console.log(`✅ ${toggleButtons.length} botones de estado configurados`);
}

async function toggleEstadoServicio(servicioId, nuevoEstado, buttonElement) {
    // Disable button during request
    buttonElement.disabled = true;
    const originalText = buttonElement.innerHTML;
    buttonElement.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Cambiando...';
    
    try {
        console.log(`📡 Enviando request AJAX para servicio ${servicioId}`);
        
        const response = await fetch(`/Servicios/CambiarEstado`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                servicioId: parseInt(servicioId),
                nuevoEstado: nuevoEstado
            })
        });
        
        if (response.ok) {
            const result = await response.json();
            console.log('✅ Estado cambiado exitosamente:', result);
            
            // Update UI immediately
            updateEstadoUI(servicioId, nuevoEstado, buttonElement);
            
            // Show success feedback
            showFeedback('success', `Estado del servicio ${nuevoEstado ? 'activado' : 'desactivado'} correctamente`);
            
            // Update statistics if modal is open
            updateStatisticsModal();
            
        } else {
            throw new Error(`Error HTTP: ${response.status}`);
        }
        
    } catch (error) {
        console.error('❌ Error cambiando estado:', error);
        
        // Show error feedback
        showFeedback('error', 'Error al cambiar el estado del servicio. Intenta nuevamente.');
        
        // Restore button
        buttonElement.innerHTML = originalText;
        
    } finally {
        buttonElement.disabled = false;
    }
}

function updateEstadoUI(servicioId, nuevoEstado, buttonElement) {
    console.log(`🎨 Actualizando UI para servicio ${servicioId}: estado ${nuevoEstado}`);
    
    // Update button
    buttonElement.dataset.estadoActual = nuevoEstado.toString();
    buttonElement.className = `btn btn-sm ${nuevoEstado ? 'btn-warning' : 'btn-success'} toggle-estado-btn`;
    buttonElement.innerHTML = `<i class="fas ${nuevoEstado ? 'fa-ban' : 'fa-check'}"></i> ${nuevoEstado ? 'Desactivar' : 'Activar'}`;
    
    // Update badge in table
    const badgeElement = document.querySelector(`#servicio-${servicioId} .estado-badge`);
    if (badgeElement) {
        badgeElement.className = `badge badge-pill ${nuevoEstado ? 'badge-success' : 'badge-danger'} estado-badge`;
        badgeElement.textContent = nuevoEstado ? 'Activo' : 'Inactivo';
    }
    
    // Update row styling
    const rowElement = document.querySelector(`#servicio-${servicioId}`);
    if (rowElement) {
        if (nuevoEstado) {
            rowElement.classList.remove('table-secondary');
        } else {
            rowElement.classList.add('table-secondary');
        }
    }
    
    console.log('✅ UI actualizada correctamente');
}

// ============================================================================
// ESTADÍSTICAS FUNCTIONALITY
// ============================================================================

function initializeStatistics() {
    console.log('📊 Inicializando estadísticas');
    
    const statsButton = document.getElementById('stats-button');
    
    if (statsButton) {
        statsButton.addEventListener('click', function() {
            console.log('📊 Mostrando modal de estadísticas');
            updateStatisticsModal();
            $('#statisticsModal').modal('show');
        });
        
        console.log('✅ Botón estadísticas configurado');
    }
}

// ============================================================================
// EXPORT EXCEL FUNCTIONALITY
// ============================================================================

function initializeExport() {
    console.log('📊 Inicializando export Excel');
    
    const exportButton = document.getElementById('export-excel-btn');
    
    if (exportButton) {
        exportButton.addEventListener('click', function() {
            console.log('📊 Iniciando export Excel con filtros actuales');
            exportToExcel();
        });
        
        console.log('✅ Botón export Excel configurado');
    }
}

function exportToExcel() {
    try {
        // Obtener valores actuales de filtros
        const filtros = getCurrentFilters();
        
        console.log('📊 Exportando con filtros:', filtros);
        
        // Mostrar loading en botón
        const exportButton = document.getElementById('export-excel-btn');
        const originalText = exportButton.innerHTML;
        exportButton.disabled = true;
        exportButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Generando...';
        
        // Construir URL con parámetros
        const params = new URLSearchParams();
        
        if (filtros.nombre) params.append('nombre', filtros.nombre);
        if (filtros.tipoServicioId) params.append('tipoServicioId', filtros.tipoServicioId);
        if (filtros.precioMin) params.append('precioMin', filtros.precioMin);
        if (filtros.precioMax) params.append('precioMax', filtros.precioMax);
        if (filtros.soloActivos) params.append('soloActivos', 'true');
        
        const exportUrl = `/Servicios/ExportarServiciosExcel?${params.toString()}`;
        
        console.log('📊 URL export:', exportUrl);
        
        // Descargar archivo
        window.location.href = exportUrl;
        
        // Mostrar mensaje éxito
        showFeedback('success', 'Excel generado correctamente. La descarga comenzará automáticamente.');
        
        // Restaurar botón después de delay
        setTimeout(() => {
            exportButton.disabled = false;
            exportButton.innerHTML = originalText;
        }, 3000);
        
    } catch (error) {
        console.error('❌ Error exportando Excel:', error);
        showFeedback('error', 'Error al generar el archivo Excel. Intenta nuevamente.');
        
        // Restaurar botón
        const exportButton = document.getElementById('export-excel-btn');
        exportButton.disabled = false;
        exportButton.innerHTML = '<i class="fas fa-file-excel"></i> Exportar Excel';
    }
}

function getCurrentFilters() {
    return {
        nombre: document.getElementById('nombre')?.value || null,
        tipoServicioId: document.getElementById('tipoServicioId')?.value || null,
        precioMin: document.getElementById('precioMin')?.value || null,
        precioMax: document.getElementById('precioMax')?.value || null,
        soloActivos: document.getElementById('soloActivos')?.checked || false
    };
}

function updateStatisticsModal() {
    console.log('📊 Actualizando estadísticas del modal');
    
    try {
        // Count services by status
        const totalServicios = document.querySelectorAll('tbody tr[id^="servicio-"]').length;
        const serviciosActivos = document.querySelectorAll('.estado-badge.badge-success').length;
        const serviciosInactivos = totalServicios - serviciosActivos;
        
        // Count by tipo servicio
        const tiposCount = {};
        document.querySelectorAll('tbody tr[id^="servicio-"]').forEach(row => {
            const tipoCell = row.querySelector('.tipo-servicio-cell');
            if (tipoCell) {
                const tipo = tipoCell.textContent.trim();
                tiposCount[tipo] = (tiposCount[tipo] || 0) + 1;
            }
        });
        
        // Calculate price statistics
        const precios = [];
        document.querySelectorAll('.precio-cell').forEach(cell => {
            const precioText = cell.textContent.replace(/[^\d.,]/g, '').replace(',', '.');
            const precio = parseFloat(precioText);
            if (!isNaN(precio)) {
                precios.push(precio);
            }
        });
        
        const precioPromedio = precios.length > 0 ? precios.reduce((a, b) => a + b, 0) / precios.length : 0;
        const precioMinimo = precios.length > 0 ? Math.min(...precios) : 0;
        const precioMaximo = precios.length > 0 ? Math.max(...precios) : 0;
        
        // Update modal content
        const modalBody = document.querySelector('#statisticsModal .modal-body');
        if (modalBody) {
            modalBody.innerHTML = `
                <div class="row">
                    <div class="col-md-6">
                        <h6><i class="fas fa-chart-bar text-primary"></i> Resumen General</h6>
                        <ul class="list-unstyled">
                            <li><strong>Total Servicios:</strong> ${totalServicios}</li>
                            <li><strong>Servicios Activos:</strong> <span class="text-success">${serviciosActivos}</span></li>
                            <li><strong>Servicios Inactivos:</strong> <span class="text-danger">${serviciosInactivos}</span></li>
                        </ul>
                    </div>
                    <div class="col-md-6">
                        <h6><i class="fas fa-dollar-sign text-success"></i> Precios</h6>
                        <ul class="list-unstyled">
                            <li><strong>Precio Promedio:</strong> $${precioPromedio.toFixed(2)}</li>
                            <li><strong>Precio Mínimo:</strong> $${precioMinimo.toFixed(2)}</li>
                            <li><strong>Precio Máximo:</strong> $${precioMaximo.toFixed(2)}</li>
                        </ul>
                    </div>
                </div>
                <div class="row mt-3">
                    <div class="col-12">
                        <h6><i class="fas fa-tags text-info"></i> Por Tipo de Servicio</h6>
                        <div class="d-flex flex-wrap">
                            ${Object.entries(tiposCount).map(([tipo, count]) => 
                                `<span class="badge badge-info mr-2 mb-2">${tipo}: ${count}</span>`
                            ).join('')}
                        </div>
                    </div>
                </div>
            `;
        }
        
        console.log('✅ Estadísticas actualizadas:', {
            total: totalServicios,
            activos: serviciosActivos,
            inactivos: serviciosInactivos,
            precioPromedio: precioPromedio.toFixed(2)
        });
        
    } catch (error) {
        console.error('❌ Error actualizando estadísticas:', error);
    }
}

// ============================================================================
// FEEDBACK SYSTEM
// ============================================================================

function showFeedback(type, message) {
    console.log(`💬 Mostrando feedback ${type}: ${message}`);
    
    // Remove existing feedback
    const existingFeedback = document.querySelector('.feedback-alert');
    if (existingFeedback) {
        existingFeedback.remove();
    }
    
    // Create new feedback
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const iconClass = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle';
    
    const feedbackHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show feedback-alert" role="alert">
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
        const feedbackElement = document.querySelector('.feedback-alert');
        if (feedbackElement) {
            feedbackElement.classList.remove('show');
            setTimeout(() => feedbackElement.remove(), 300);
        }
    }, 5000);
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

function formatCurrency(amount) {
    return new Intl.NumberFormat('es-CL', {
        style: 'currency',
        currency: 'CLP',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount);
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

console.log('📋 servicios-index.js: Todas las funciones definidas correctamente');
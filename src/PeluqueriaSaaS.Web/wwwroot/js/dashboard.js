    /*
 * Archivo: wwwroot/js/dashboard.js
 * Propósito: Dashboard con Chart.js - gráficos interactivos + datos AJAX
 * Fecha: Julio 2025
 * Features: Line chart ventas + Bar chart servicios + Pie chart tipos
 */

console.log('📊 dashboard.js cargado correctamente');

// Variables globales para charts
let ventasChart, serviciosChart, tiposChart;

document.addEventListener('DOMContentLoaded', function() {
    console.log('🏠 dashboard.js: DOM ready - inicializando dashboard');
    
    // Inicializar funcionalidades
    initializeDashboard();
    
    console.log('✅ dashboard.js: Dashboard inicializado correctamente');
});

// ============================================================================
// DASHBOARD INITIALIZATION
// ============================================================================

async function initializeDashboard() {
    console.log('🚀 Inicializando dashboard completo');
    
    try {
        // Cargar datos del dashboard
        const data = await loadDashboardData();
        
        if (data && !data.error) {
            // Inicializar todos los gráficos
            initializeVentasChart(data.ventasUltimos30Dias);
            initializeServiciosChart(data.topServicios);
            initializeTiposChart(data.serviciosPorTipo);
            
            // Configurar botones de actualización
            setupRefreshButtons();
            
            console.log('✅ Dashboard inicializado exitosamente');
        } else {
            console.error('❌ Error cargando datos dashboard:', data?.error);
            showErrorMessage('Error cargando datos del dashboard');
        }
    } catch (error) {
        console.error('❌ Error inicializando dashboard:', error);
        showErrorMessage('Error inicializando dashboard');
    }
}

// ============================================================================
// DATA LOADING
// ============================================================================

async function loadDashboardData() {
    try {
        console.log('📡 Cargando datos dashboard via AJAX');
        
        const response = await fetch('/Home/GetDashboardData');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        console.log('✅ Datos dashboard cargados:', data);
        
        return data;
    } catch (error) {
        console.error('❌ Error cargando datos dashboard:', error);
        return { error: error.message };
    }
}

// ============================================================================
// VENTAS CHART (LINE CHART)
// ============================================================================

function initializeVentasChart(ventasData) {
    console.log('📈 Inicializando gráfico ventas');
    
    const ctx = document.getElementById('ventasChart');
    if (!ctx) {
        console.warn('⚠️ Canvas ventasChart no encontrado');
        return;
    }
    
    // Preparar datos
    const labels = ventasData.map(v => v.fecha);
    const totales = ventasData.map(v => v.total);
    const cantidades = ventasData.map(v => v.cantidad);
    
    ventasChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Ingresos ($)',
                data: totales,
                borderColor: 'rgb(54, 162, 235)',
                backgroundColor: 'rgba(54, 162, 235, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                yAxisID: 'y'
            }, {
                label: 'Cantidad Ventas',
                data: cantidades,
                borderColor: 'rgb(255, 99, 132)',
                backgroundColor: 'rgba(255, 99, 132, 0.1)',
                borderWidth: 2,
                fill: false,
                tension: 0.4,
                yAxisID: 'y1'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            plugins: {
                title: {
                    display: true,
                    text: 'Tendencia de Ventas - Últimos 30 Días'
                },
                legend: {
                    display: true,
                    position: 'top'
                }
            },
            scales: {
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Fecha'
                    }
                },
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    title: {
                        display: true,
                        text: 'Ingresos ($)'
                    }
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    title: {
                        display: true,
                        text: 'Cantidad'
                    },
                    grid: {
                        drawOnChartArea: false,
                    },
                }
            }
        }
    });
    
    console.log('✅ Gráfico ventas inicializado');
}

// ============================================================================
// SERVICIOS CHART (DOUGHNUT CHART)
// ============================================================================

function initializeServiciosChart(serviciosData) {
    console.log('🍩 Inicializando gráfico servicios');
    
    const ctx = document.getElementById('serviciosChart');
    if (!ctx) {
        console.warn('⚠️ Canvas serviciosChart no encontrado');
        return;
    }
    
    // Preparar datos
    const labels = serviciosData.map(s => s.nombre);
    const ventas = serviciosData.map(s => s.ventas);
    
    // Colores dinámicos
    const colors = [
        'rgba(255, 99, 132, 0.8)',
        'rgba(54, 162, 235, 0.8)',
        'rgba(255, 205, 86, 0.8)',
        'rgba(75, 192, 192, 0.8)',
        'rgba(153, 102, 255, 0.8)'
    ];
    
    serviciosChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: ventas,
                backgroundColor: colors,
                borderColor: colors.map(color => color.replace('0.8', '1')),
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: 'Servicios Más Populares'
                },
                legend: {
                    display: true,
                    position: 'bottom'
                }
            }
        }
    });
    
    console.log('✅ Gráfico servicios inicializado');
}

// ============================================================================
// TIPOS CHART (BAR CHART)
// ============================================================================

function initializeTiposChart(tiposData) {
    console.log('📊 Inicializando gráfico tipos servicios');
    
    const ctx = document.getElementById('tiposChart');
    if (!ctx) {
        console.warn('⚠️ Canvas tiposChart no encontrado');
        return;
    }
    
    // Preparar datos
    const labels = tiposData.map(t => t.tipo);
    const cantidades = tiposData.map(t => t.cantidad);
    const precios = tiposData.map(t => t.promedioPrecio);
    
    tiposChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Cantidad Servicios',
                data: cantidades,
                backgroundColor: 'rgba(54, 162, 235, 0.8)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1,
                yAxisID: 'y'
            }, {
                label: 'Precio Promedio ($)',
                data: precios,
                backgroundColor: 'rgba(255, 159, 64, 0.8)',
                borderColor: 'rgba(255, 159, 64, 1)',
                borderWidth: 1,
                yAxisID: 'y1'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: 'Distribución por Tipo de Servicio'
                },
                legend: {
                    display: true,
                    position: 'top'
                }
            },
            scales: {
                x: {
                    display: true
                },
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    title: {
                        display: true,
                        text: 'Cantidad'
                    }
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    title: {
                        display: true,
                        text: 'Precio Promedio ($)'
                    },
                    grid: {
                        drawOnChartArea: false,
                    }
                }
            }
        }
    });
    
    console.log('✅ Gráfico tipos servicios inicializado');
}

// ============================================================================
// REFRESH FUNCTIONALITY
// ============================================================================

function setupRefreshButtons() {
    console.log('🔄 Configurando botones refresh');
    
    const refreshVentasBtn = document.getElementById('refresh-ventas-btn');
    
    if (refreshVentasBtn) {
        refreshVentasBtn.addEventListener('click', async function() {
            console.log('🔄 Refrescando datos dashboard');
            
            // Mostrar loading
            const originalText = this.innerHTML;
            this.disabled = true;
            this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Actualizando...';
            
            try {
                // Recargar datos
                const data = await loadDashboardData();
                
                if (data && !data.error) {
                    // Actualizar gráficos
                    updateVentasChart(data.ventasUltimos30Dias);
                    updateServiciosChart(data.topServicios);
                    updateTiposChart(data.serviciosPorTipo);
                    
                    showSuccessMessage('Datos actualizados correctamente');
                } else {
                    showErrorMessage('Error actualizando datos');
                }
            } catch (error) {
                console.error('❌ Error refrescando datos:', error);
                showErrorMessage('Error actualizando dashboard');
            } finally {
                // Restaurar botón
                this.disabled = false;
                this.innerHTML = originalText;
            }
        });
        
        console.log('✅ Botón refresh configurado');
    }
}

// ============================================================================
// CHART UPDATE FUNCTIONS
// ============================================================================

function updateVentasChart(ventasData) {
    if (ventasChart) {
        ventasChart.data.labels = ventasData.map(v => v.fecha);
        ventasChart.data.datasets[0].data = ventasData.map(v => v.total);
        ventasChart.data.datasets[1].data = ventasData.map(v => v.cantidad);
        ventasChart.update();
        console.log('✅ Gráfico ventas actualizado');
    }
}

function updateServiciosChart(serviciosData) {
    if (serviciosChart) {
        serviciosChart.data.labels = serviciosData.map(s => s.nombre);
        serviciosChart.data.datasets[0].data = serviciosData.map(s => s.ventas);
        serviciosChart.update();
        console.log('✅ Gráfico servicios actualizado');
    }
}

function updateTiposChart(tiposData) {
    if (tiposChart) {
        tiposChart.data.labels = tiposData.map(t => t.tipo);
        tiposChart.data.datasets[0].data = tiposData.map(t => t.cantidad);
        tiposChart.data.datasets[1].data = tiposData.map(t => t.promedioPrecio);
        tiposChart.update();
        console.log('✅ Gráfico tipos actualizado');
    }
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

function showSuccessMessage(message) {
    console.log(`✅ ${message}`);
    
    // Crear toast o alert temporal
    const alert = document.createElement('div');
    alert.className = 'alert alert-success alert-dismissible fade show position-fixed';
    alert.style.top = '20px';
    alert.style.right = '20px';
    alert.style.zIndex = '9999';
    alert.innerHTML = `
        <i class="fas fa-check-circle"></i> ${message}
        <button type="button" class="close" data-dismiss="alert">
            <span>&times;</span>
        </button>
    `;
    
    document.body.appendChild(alert);
    
    // Auto-remove después de 3 segundos
    setTimeout(() => {
        if (alert.parentNode) {
            alert.parentNode.removeChild(alert);
        }
    }, 3000);
}

function showErrorMessage(message) {
    console.error(`❌ ${message}`);
    
    // Crear toast o alert temporal
    const alert = document.createElement('div');
    alert.className = 'alert alert-danger alert-dismissible fade show position-fixed';
    alert.style.top = '20px';
    alert.style.right = '20px';
    alert.style.zIndex = '9999';
    alert.innerHTML = `
        <i class="fas fa-exclamation-triangle"></i> ${message}
        <button type="button" class="close" data-dismiss="alert">
            <span>&times;</span>
        </button>
    `;
    
    document.body.appendChild(alert);
    
    // Auto-remove después de 5 segundos
    setTimeout(() => {
        if (alert.parentNode) {
            alert.parentNode.removeChild(alert);
        }
    }, 5000);
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('es-UY', {
        style: 'currency',
        currency: 'UYU',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount);
}

function formatNumber(number) {
    return new Intl.NumberFormat('es-UY').format(number);
}

// ============================================================================
// RESPONSIVE HANDLING
// ============================================================================

window.addEventListener('resize', function() {
    // Resize charts on window resize
    if (ventasChart) ventasChart.resize();
    if (serviciosChart) serviciosChart.resize();
    if (tiposChart) tiposChart.resize();
});

console.log('📊 dashboard.js: Todas las funciones definidas correctamente');
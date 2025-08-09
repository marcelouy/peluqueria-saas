// articulos-calculos.js - Sistema de cálculo dinámico para formulario de Artículos
// Ubicación: wwwroot/js/articulos-calculos.js
// Fecha: Agosto 2025
// Premisa: TODO el JavaScript en archivos separados, NUNCA inline

(function () {
    'use strict';

    // Elementos del formulario - compatibles con nombres actuales
    let costoInput;
    let precioInput;
    let margenInput;
    let margenDisplay;
    
    // Flag para prevenir loops de cálculo
    let isCalculating = false;

    // Inicialización cuando el DOM está listo
    document.addEventListener('DOMContentLoaded', function () {
        initializeElements();
        if (areElementsValid()) {
            attachEventListeners();
            // Calcular margen inicial si hay valores
            calculateInitialMargin();
        }
    });

    function initializeElements() {
        // Buscar elementos por diferentes nombres posibles
        costoInput = document.getElementById('PrecioCosto') || 
                    document.getElementById('Costo') ||
                    document.querySelector('input[name="PrecioCosto"]');
                    
        precioInput = document.getElementById('Precio') || 
                     document.getElementById('PrecioVenta') ||
                     document.querySelector('input[name="Precio"]');
                     
        margenInput = document.getElementById('Margen') || 
                     document.getElementById('MargenGanancia') ||
                     document.getElementById('margenCalculado');
        
        // Crear display visual del margen si no existe
        if (margenInput) {
            createMargenDisplay();
        }
    }

    function createMargenDisplay() {
        // Buscar si ya existe el display
        margenDisplay = document.getElementById('margenDisplay');
        
        if (!margenDisplay && margenInput) {
            const margenContainer = margenInput.closest('.mb-3') || margenInput.closest('.form-group');
            if (margenContainer) {
                // Verificar si ya existe un form-text
                margenDisplay = margenContainer.querySelector('.form-text');
                
                if (!margenDisplay) {
                    margenDisplay = document.createElement('div');
                    margenDisplay.id = 'margenDisplay';
                    margenDisplay.className = 'form-text mt-1';
                    margenContainer.appendChild(margenDisplay);
                }
            }
        }
    }

    function areElementsValid() {
        if (!costoInput || !precioInput) {
            console.warn('Elementos de costo o precio no encontrados');
            return false;
        }
        return true;
    }

    function attachEventListeners() {
        // Escuchar cambios en Costo
        if (costoInput) {
            costoInput.addEventListener('input', handleCostoChange);
            costoInput.addEventListener('change', handleCostoChange);
        }
        
        // Escuchar cambios en Precio
        if (precioInput) {
            precioInput.addEventListener('input', handlePrecioChange);
            precioInput.addEventListener('change', handlePrecioChange);
        }
        
        // Si el margen es editable, escuchar cambios
        if (margenInput && !margenInput.hasAttribute('readonly')) {
            margenInput.addEventListener('input', handleMargenChange);
            margenInput.addEventListener('change', handleMargenChange);
        }
    }

    function handleCostoChange() {
        if (isCalculating) return;
        
        const costo = parseFloat(costoInput.value) || 0;
        const precio = parseFloat(precioInput.value) || 0;
        
        if (costo > 0 && precio > 0) {
            // Calcular margen cuando cambia el costo
            calculateMargenFromCostoPrecio(costo, precio);
        }
        
        updateMargenDisplay();
    }

    function handlePrecioChange() {
        if (isCalculating) return;
        
        const costo = parseFloat(costoInput.value) || 0;
        const precio = parseFloat(precioInput.value) || 0;
        
        if (costo > 0 && precio > 0) {
            // Calcular margen cuando cambia el precio
            calculateMargenFromCostoPrecio(costo, precio);
        }
        
        updateMargenDisplay();
    }

    function handleMargenChange() {
        if (isCalculating) return;
        
        const costo = parseFloat(costoInput.value) || 0;
        const margen = parseFloat(margenInput.value) || 0;
        
        if (costo > 0) {
            // Calcular precio cuando cambia el margen
            calculatePrecioFromCostoMargen(costo, margen);
        }
        
        updateMargenDisplay();
    }

    function calculatePrecioFromCostoMargen(costo, margen) {
        isCalculating = true;
        
        // Precio = Costo * (1 + Margen/100)
        const precio = costo * (1 + margen / 100);
        
        if (precioInput) {
            precioInput.value = precio.toFixed(2);
            // Trigger change event para validación
            precioInput.dispatchEvent(new Event('change', { bubbles: true }));
        }
        
        setTimeout(() => { isCalculating = false; }, 100);
    }

    function calculateMargenFromCostoPrecio(costo, precio) {
        isCalculating = true;
        
        // Margen = ((Precio - Costo) / Costo) * 100
        const margen = ((precio - costo) / costo) * 100;
        
        if (margenInput) {
            margenInput.value = margen.toFixed(2);
            
            // Si el margen es readonly, actualizar como texto
            if (margenInput.hasAttribute('readonly')) {
                margenInput.value = margen.toFixed(2) + '%';
            } else {
                margenInput.value = margen.toFixed(2);
            }
            
            // Trigger change event si no es readonly
            if (!margenInput.hasAttribute('readonly')) {
                margenInput.dispatchEvent(new Event('change', { bubbles: true }));
            }
        }
        
        setTimeout(() => { isCalculating = false; }, 100);
    }

    function calculateInitialMargin() {
        const costo = parseFloat(costoInput?.value) || 0;
        const precio = parseFloat(precioInput?.value) || 0;
        
        if (costo > 0 && precio > 0) {
            calculateMargenFromCostoPrecio(costo, precio);
            updateMargenDisplay();
        }
    }

    function updateMargenDisplay() {
        if (!margenDisplay) return;
        
        const costo = parseFloat(costoInput?.value) || 0;
        const precio = parseFloat(precioInput?.value) || 0;
        
        if (costo > 0 && precio > 0) {
            const ganancia = precio - costo;
            const margenCalculado = ((ganancia / costo) * 100).toFixed(2);
            
            let displayClass = 'text-success';
            let icon = '✓';
            let mensaje = '';
            
            if (margenCalculado < 10) {
                displayClass = 'text-warning';
                icon = '⚠';
                mensaje = 'Margen bajo';
            } 
            
            if (margenCalculado < 0) {
                displayClass = 'text-danger';
                icon = '✗';
                mensaje = '¡Pérdida!';
            }
            
            if (margenCalculado > 50) {
                displayClass = 'text-success fw-bold';
                icon = '★';
                mensaje = 'Excelente margen';
            }
            
            margenDisplay.innerHTML = `
                <span class="${displayClass}">
                    ${icon} Ganancia: $${ganancia.toFixed(2)} (${margenCalculado}%)
                    ${mensaje ? `<br><small>${mensaje}</small>` : ''}
                </span>
            `;
        } else if (costo > 0 || precio > 0) {
            margenDisplay.innerHTML = '<span class="text-muted"><small>Ingrese costo y precio para ver el margen</small></span>';
        } else {
            margenDisplay.innerHTML = '';
        }
    }

    // Función helper para formatear números
    function formatNumber(value) {
        return new Intl.NumberFormat('es-UY', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(value);
    }

    // Manejar campo de oferta si existe
    function handleOfertaToggle() {
        const ofertaCheckbox = document.getElementById('Oferta');
        const precioOfertaRow = document.getElementById('precioOfertaRow');
        const precioOfertaInput = document.getElementById('PrecioOferta');
        
        if (ofertaCheckbox && precioOfertaRow) {
            // Mostrar/ocultar precio de oferta
            ofertaCheckbox.addEventListener('change', function() {
                if (this.checked) {
                    precioOfertaRow.style.display = 'flex';
                    // Si no hay precio de oferta, sugerir uno con descuento
                    if (precioOfertaInput && !precioOfertaInput.value && precioInput) {
                        const precioActual = parseFloat(precioInput.value) || 0;
                        if (precioActual > 0) {
                            // Sugerir 10% de descuento
                            precioOfertaInput.value = (precioActual * 0.9).toFixed(2);
                        }
                    }
                } else {
                    precioOfertaRow.style.display = 'none';
                    if (precioOfertaInput) {
                        precioOfertaInput.value = '';
                    }
                }
            });
            
            // Ejecutar al cargar si ya está marcado
            if (ofertaCheckbox.checked) {
                precioOfertaRow.style.display = 'flex';
            }
        }
    }

    // Manejar control de stock
    function handleStockControl() {
        const requiereStockCheckbox = document.getElementById('RequiereStock');
        const stockActualDiv = document.getElementById('stockActualDiv');
        const stockMinimoDiv = document.getElementById('stockMinimoDiv');
        
        if (requiereStockCheckbox) {
            requiereStockCheckbox.addEventListener('change', function() {
                const display = this.checked ? 'block' : 'none';
                if (stockActualDiv) stockActualDiv.style.display = display;
                if (stockMinimoDiv) stockMinimoDiv.style.display = display;
            });
            
            // Estado inicial
            if (!requiereStockCheckbox.checked) {
                if (stockActualDiv) stockActualDiv.style.display = 'none';
                if (stockMinimoDiv) stockMinimoDiv.style.display = 'none';
            }
        }
    }

    // Inicializar handlers adicionales
    document.addEventListener('DOMContentLoaded', function() {
        handleOfertaToggle();
        handleStockControl();
    });

    // Exponer funciones para uso externo si es necesario
    window.ArticulosCalculos = {
        recalculate: function() {
            calculateInitialMargin();
            updateMargenDisplay();
        },
        formatCurrency: function(value) {
            return formatNumber(value);
        }
    };

})();
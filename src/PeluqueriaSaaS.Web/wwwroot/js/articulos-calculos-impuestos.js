// articulos-calculos-impuestos.js - Sistema de cálculo con impuestos en cascada
// Ubicación: wwwroot/js/articulos-calculos-impuestos.js
// Fecha: Agosto 2025
// Soporta: Tributo → Margen → IVA

(function () {
    'use strict';

    // Elementos del formulario
    let costoInput;
    let precioInput;
    let margenInput;
    let margenDisplay;
    let tributoCheckbox;
    let ivaSelect;
    
    // Valores de impuestos (podrían venir del servidor)
    const TRIBUTO_PROFESIONAL = 2; // 2%
    const IVA_TASAS = {
        'IVA_0': 0,
        'IVA_10': 10,
        'IVA_22': 22
    };
    
    // Flag para prevenir loops
    let isCalculating = false;

    document.addEventListener('DOMContentLoaded', function () {
        initializeElements();
        if (areElementsValid()) {
            attachEventListeners();
            calculateInitialValues();
        }
    });

    function initializeElements() {
        costoInput = document.getElementById('PrecioCosto') || document.getElementById('Costo');
        precioInput = document.getElementById('Precio') || document.getElementById('PrecioVenta');
        margenInput = document.getElementById('Margen') || document.getElementById('margenCalculado');
        tributoCheckbox = document.getElementById('AplicaTributo');
        ivaSelect = document.getElementById('TipoIVA');
        
        createMargenDisplay();
        createImpuestosDisplay();
    }

    function createMargenDisplay() {
        margenDisplay = document.getElementById('margenDisplay');
        if (!margenDisplay && margenInput) {
            const container = margenInput.closest('.mb-3');
            if (container) {
                margenDisplay = document.createElement('div');
                margenDisplay.id = 'margenDisplay';
                margenDisplay.className = 'form-text mt-1';
                container.appendChild(margenDisplay);
            }
        }
    }

    function createImpuestosDisplay() {
        // Crear display para desglose de impuestos
        const precioContainer = precioInput?.closest('.mb-3');
        if (precioContainer && !document.getElementById('impuestosDisplay')) {
            const display = document.createElement('div');
            display.id = 'impuestosDisplay';
            display.className = 'alert alert-info mt-2 p-2';
            display.style.fontSize = '0.9em';
            precioContainer.appendChild(display);
        }
    }

    function areElementsValid() {
        return costoInput && precioInput;
    }

    function attachEventListeners() {
        // Listeners para campos principales
        costoInput?.addEventListener('input', handleCostoChange);
        precioInput?.addEventListener('input', handlePrecioChange);
        margenInput?.addEventListener('input', handleMargenChange);
        
        // Listeners para impuestos
        tributoCheckbox?.addEventListener('change', recalcularTodo);
        ivaSelect?.addEventListener('change', recalcularTodo);
    }

    function handleCostoChange() {
        if (isCalculating) return;
        recalcularDesdeMargen();
    }

    function handleMargenChange() {
        if (isCalculating) return;
        recalcularDesdeMargen();
    }

    function handlePrecioChange() {
        if (isCalculating) return;
        recalcularMargenDesdePrecio();
    }

    function recalcularDesdeMargen() {
        const costo = parseFloat(costoInput?.value) || 0;
        const margen = parseFloat(margenInput?.value) || 0;
        
        if (costo <= 0) return;
        
        isCalculating = true;
        
        // Obtener impuestos seleccionados
        const aplicaTributo = tributoCheckbox?.checked || false;
        const tasaIVA = getIVASeleccionado();
        
        // Calcular precio con cascada de impuestos
        let precioCalculado = costo;
        
        // 1. Aplicar tributo profesional si corresponde
        if (aplicaTributo) {
            precioCalculado = precioCalculado * (1 + TRIBUTO_PROFESIONAL / 100);
        }
        
        // 2. Aplicar margen de ganancia
        precioCalculado = precioCalculado * (1 + margen / 100);
        
        // Este es el precio SIN IVA
        const precioSinIVA = precioCalculado;
        
        // 3. Aplicar IVA
        const precioConIVA = precioSinIVA * (1 + tasaIVA / 100);
        
        // Actualizar campo de precio (sin IVA)
        if (precioInput) {
            precioInput.value = precioSinIVA.toFixed(2);
        }
        
        // Mostrar desglose
        mostrarDesglose(costo, aplicaTributo, margen, tasaIVA, precioSinIVA, precioConIVA);
        
        setTimeout(() => { isCalculating = false; }, 100);
    }

    function recalcularMargenDesdePrecio() {
        const costo = parseFloat(costoInput?.value) || 0;
        const precioSinIVA = parseFloat(precioInput?.value) || 0;
        
        if (costo <= 0 || precioSinIVA <= 0) return;
        
        isCalculating = true;
        
        // Obtener impuestos
        const aplicaTributo = tributoCheckbox?.checked || false;
        const tasaIVA = getIVASeleccionado();
        
        // Calcular margen inverso
        let costoConTributo = costo;
        if (aplicaTributo) {
            costoConTributo = costo * (1 + TRIBUTO_PROFESIONAL / 100);
        }
        
        // Margen = ((PrecioSinIVA - CostoConTributo) / CostoConTributo) * 100
        const margen = ((precioSinIVA - costoConTributo) / costoConTributo) * 100;
        
        if (margenInput) {
            margenInput.value = margen.toFixed(2);
        }
        
        // Calcular precio con IVA para el display
        const precioConIVA = precioSinIVA * (1 + tasaIVA / 100);
        
        // Mostrar desglose
        mostrarDesglose(costo, aplicaTributo, margen, tasaIVA, precioSinIVA, precioConIVA);
        
        setTimeout(() => { isCalculating = false; }, 100);
    }

    function recalcularTodo() {
        // Recalcular cuando cambian los checkboxes de impuestos
        const margen = parseFloat(margenInput?.value) || 0;
        if (margen > 0) {
            recalcularDesdeMargen();
        } else {
            recalcularMargenDesdePrecio();
        }
    }

    function getIVASeleccionado() {
        if (ivaSelect) {
            return IVA_TASAS[ivaSelect.value] || 22;
        }
        return 22; // IVA por defecto
    }

    function mostrarDesglose(costo, aplicaTributo, margen, tasaIVA, precioSinIVA, precioConIVA) {
        const display = document.getElementById('impuestosDisplay');
        if (!display) return;
        
        let html = '<strong>Desglose de cálculo:</strong><br>';
        html += `Costo base: $${costo.toFixed(2)}<br>`;
        
        if (aplicaTributo) {
            const costoConTributo = costo * (1 + TRIBUTO_PROFESIONAL / 100);
            html += `+ Tributo Prof. (${TRIBUTO_PROFESIONAL}%): $${(costoConTributo - costo).toFixed(2)}<br>`;
            html += `= Costo c/tributo: $${costoConTributo.toFixed(2)}<br>`;
        }
        
        html += `+ Margen (${margen.toFixed(1)}%): $${(precioSinIVA - (aplicaTributo ? costo * 1.02 : costo)).toFixed(2)}<br>`;
        html += `<strong>= Precio sin IVA: $${precioSinIVA.toFixed(2)}</strong><br>`;
        html += `+ IVA (${tasaIVA}%): $${(precioConIVA - precioSinIVA).toFixed(2)}<br>`;
        html += `<strong class="text-primary">= Precio final: $${precioConIVA.toFixed(2)}</strong>`;
        
        display.innerHTML = html;
        
        // Actualizar display de margen con colores
        updateMargenDisplay(margen);
    }

    function updateMargenDisplay(margen) {
        if (!margenDisplay) return;
        
        let displayClass = 'text-success';
        let icon = '✓';
        let mensaje = '';
        
        if (margen < 10) {
            displayClass = 'text-warning';
            icon = '⚠';
            mensaje = 'Margen bajo';
        }
        
        if (margen < 0) {
            displayClass = 'text-danger';
            icon = '✗';
            mensaje = '¡Pérdida!';
        }
        
        if (margen > 50) {
            displayClass = 'text-success fw-bold';
            icon = '★';
            mensaje = 'Excelente margen';
        }
        
        margenDisplay.innerHTML = `
            <span class="${displayClass}">
                ${icon} Margen: ${margen.toFixed(2)}%
                ${mensaje ? ` - ${mensaje}` : ''}
            </span>
        `;
    }

    function calculateInitialValues() {
        const costo = parseFloat(costoInput?.value) || 0;
        const precio = parseFloat(precioInput?.value) || 0;
        
        if (costo > 0 && precio > 0) {
            recalcularMargenDesdePrecio();
        }
    }

    // Exponer funciones públicas
    window.ArticulosCalculosImpuestos = {
        recalculate: function() {
            calculateInitialValues();
        },
        getDesglose: function() {
            const costo = parseFloat(costoInput?.value) || 0;
            const margen = parseFloat(margenInput?.value) || 0;
            const aplicaTributo = tributoCheckbox?.checked || false;
            const tasaIVA = getIVASeleccionado();
            
            let resultado = {
                costo: costo,
                tributo: aplicaTributo ? costo * (TRIBUTO_PROFESIONAL / 100) : 0,
                margen: 0,
                precioSinIVA: 0,
                iva: 0,
                precioFinal: 0
            };
            
            let base = costo;
            if (aplicaTributo) {
                base = costo * (1 + TRIBUTO_PROFESIONAL / 100);
            }
            
            resultado.precioSinIVA = base * (1 + margen / 100);
            resultado.margen = resultado.precioSinIVA - base;
            resultado.iva = resultado.precioSinIVA * (tasaIVA / 100);
            resultado.precioFinal = resultado.precioSinIVA + resultado.iva;
            
            return resultado;
        }
    };

})();
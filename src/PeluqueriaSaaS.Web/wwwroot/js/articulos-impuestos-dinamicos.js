// articulos-impuestos-dinamicos.js
// Ubicación: wwwroot/js/articulos-impuestos-dinamicos.js
// Sistema de cálculo dinámico de impuestos para artículos
// Fecha: Agosto 2025

(function () {
    'use strict';

    // Elementos del DOM
    let precioInput;
    let impuestosCheckboxes;
    let precioBaseSpan;
    let precioFinalSpan;
    let desgloseDiv;

    document.addEventListener('DOMContentLoaded', function () {
        initializeElements();
        attachEventListeners();
        // IMPORTANTE: Ejecutar cálculo inicial después de un pequeño delay
        // para asegurar que todos los elementos estén listos
        setTimeout(function() {
            calcularPrecioConImpuestos();
        }, 100);
    });

    function initializeElements() {
        precioInput = document.getElementById('Precio');
        impuestosCheckboxes = document.querySelectorAll('.impuesto-check');
        precioBaseSpan = document.getElementById('precioBase');
        precioFinalSpan = document.getElementById('precioFinal');
        desgloseDiv = document.getElementById('desgloseImpuestos');
        
        // Debug inicial
        console.log('Impuestos encontrados:', impuestosCheckboxes.length);
        impuestosCheckboxes.forEach(checkbox => {
            if (checkbox.checked) {
                console.log('Impuesto marcado al inicio:', checkbox.dataset.nombre, checkbox.dataset.porcentaje + '%');
            }
        });
    }

    function attachEventListeners() {
        // Listener para cambios en el precio
        if (precioInput) {
            precioInput.addEventListener('input', calcularPrecioConImpuestos);
        }

        // Listeners para cambios en impuestos
        impuestosCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', function() {
                // Si es IVA (radio button), no necesita manejo especial
                // Si es checkbox, permitir múltiple selección
                calcularPrecioConImpuestos();
            });
        });
    }

    function calcularPrecioConImpuestos() {
        const precioBase = parseFloat(precioInput?.value) || 0;
        
        if (precioBase <= 0) {
            actualizarDisplay(0, 0, []);
            return;
        }

        // Obtener impuestos seleccionados
        const impuestosSeleccionados = obtenerImpuestosSeleccionados();
        
        // Calcular precio final con impuestos
        let precioFinal = precioBase;
        let desglose = [];
        let totalImpuestos = 0;

        // Ordenar impuestos por tipo (IVA al final)
        impuestosSeleccionados.sort((a, b) => {
            if (a.tipo === 'IVA') return 1;
            if (b.tipo === 'IVA') return -1;
            return 0;
        });

        // Aplicar cada impuesto
        impuestosSeleccionados.forEach(impuesto => {
            const montoImpuesto = precioBase * (impuesto.porcentaje / 100);
            totalImpuestos += montoImpuesto;
            
            desglose.push({
                nombre: impuesto.nombre,
                porcentaje: impuesto.porcentaje,
                monto: montoImpuesto
            });
        });

        precioFinal = precioBase + totalImpuestos;

        // Actualizar display
        actualizarDisplay(precioBase, precioFinal, desglose);
    }

    function obtenerImpuestosSeleccionados() {
        const impuestos = [];
        
        impuestosCheckboxes.forEach(checkbox => {
            if (checkbox.checked) {
                impuestos.push({
                    id: checkbox.value,
                    tipo: checkbox.dataset.tipo,
                    porcentaje: parseFloat(checkbox.dataset.porcentaje) || 0,
                    nombre: checkbox.dataset.nombre || 'Impuesto'
                });
            }
        });

        return impuestos;
    }

    function actualizarDisplay(precioBase, precioFinal, desglose) {
        // Actualizar precio base
        if (precioBaseSpan) {
            precioBaseSpan.textContent = precioBase.toFixed(2);
        }

        // Actualizar precio final
        if (precioFinalSpan) {
            precioFinalSpan.textContent = precioFinal.toFixed(2);
        }

        // Actualizar desglose de impuestos
        if (desgloseDiv) {
            if (desglose.length > 0) {
                let html = '<div class="mt-2">';
                html += '<strong>Impuestos aplicados:</strong><br>';
                
                desglose.forEach(item => {
                    html += `
                        <div class="d-flex justify-content-between">
                            <span>${item.nombre} (${item.porcentaje}%):</span>
                            <span>+$${item.monto.toFixed(2)}</span>
                        </div>
                    `;
                });
                
                const totalImpuestos = desglose.reduce((sum, item) => sum + item.monto, 0);
                html += `
                    <div class="d-flex justify-content-between mt-1 pt-1 border-top">
                        <strong>Total impuestos:</strong>
                        <strong>+$${totalImpuestos.toFixed(2)}</strong>
                    </div>
                `;
                html += '</div>';
                
                desgloseDiv.innerHTML = html;
            } else {
                desgloseDiv.innerHTML = '<em class="text-muted">Sin impuestos aplicados</em>';
            }
        }
    }

    // Exponer función global para ser llamada desde otros scripts
    window.calcularPrecioConImpuestos = calcularPrecioConImpuestos;

})();
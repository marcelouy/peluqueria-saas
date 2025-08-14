// ========================================
// FUNCIONES POS - PUNTO DE VENTA
// Manejo completo de servicios y venta
// Ubicación: wwwroot/js/pos-funciones.js
// ========================================

$(document).ready(function() {
    let serviciosEnVenta = [];
    let contadorServicios = 0;
    
    // Exponer serviciosEnVenta globalmente para validación
    window.exponerServiciosEnVenta(serviciosEnVenta);

    // Agregar servicio a la venta
    $('.btn-agregar-servicio').click(function() {
        const servicioId = $(this).data('servicio-id');
        const servicioNombre = $(this).data('servicio-nombre');
        const servicioPrecio = parseFloat($(this).data('servicio-precio'));
        const servicioTipo = $(this).data('servicio-tipo');

        // Verificar si el servicio ya está en la venta
        const servicioExistente = serviciosEnVenta.find(s => s.servicioId === servicioId);
        
        if (servicioExistente) {
            // Incrementar cantidad
            servicioExistente.cantidad++;
            actualizarFilaServicio(servicioExistente);
        } else {
            // Agregar nuevo servicio
            const nuevoServicio = {
                id: contadorServicios++,
                servicioId: servicioId,
                nombre: servicioNombre,
                precio: servicioPrecio,
                cantidad: 1,
                tipo: servicioTipo
            };
            
            serviciosEnVenta.push(nuevoServicio);
            agregarFilaServicio(nuevoServicio);
        }
        
        actualizarTotales();
        actualizarHiddenInputs();
        
        // Actualizar variable global para validación
        window.exponerServiciosEnVenta(serviciosEnVenta);
        
        // Guardar estado automáticamente
        if (window.VentasValidacion) {
            window.VentasValidacion.guardarEstado();
        }
    });

    // Agregar fila de servicio a la tabla
    function agregarFilaServicio(servicio) {
        const fila = `
            <tr data-servicio-id="${servicio.id}">
                <td>
                    <strong>${servicio.nombre}</strong><br>
                    <small class="text-muted">${servicio.tipo}</small>
                </td>
                <td>$${servicio.precio.toFixed(2)}</td>
                <td>
                    <div class="input-group input-group-sm" style="width: 100px;">
                        <button class="btn btn-outline-secondary btn-cantidad" type="button" data-action="minus">-</button>
                        <input type="number" class="form-control text-center cantidad-input" value="${servicio.cantidad}" min="1" readonly>
                        <button class="btn btn-outline-secondary btn-cantidad" type="button" data-action="plus">+</button>
                    </div>
                </td>
                <td class="subtotal-servicio">$${(servicio.precio * servicio.cantidad).toFixed(2)}</td>
                <td>
                    <button type="button" class="btn btn-sm btn-outline-danger btn-eliminar-servicio">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
        
        $('#serviciosVenta').append(fila);
        $('#noServicios').hide();
        
        // Mostrar toast de servicio agregado
        if (window.toastr) {
            toastr.success(`${servicio.nombre} agregado`, 'Servicio agregado', {
                timeOut: 2000,
                progressBar: true
            });
        }
    }

    // Actualizar fila de servicio existente
    function actualizarFilaServicio(servicio) {
        const fila = $(`tr[data-servicio-id="${servicio.id}"]`);
        fila.find('.cantidad-input').val(servicio.cantidad);
        fila.find('.subtotal-servicio').text(`$${(servicio.precio * servicio.cantidad).toFixed(2)}`);
    }

    // Eventos para cambiar cantidad
    $(document).on('click', '.btn-cantidad', function() {
        const action = $(this).data('action');
        const fila = $(this).closest('tr');
        const servicioId = parseInt(fila.data('servicio-id'));
        const servicio = serviciosEnVenta.find(s => s.id === servicioId);
        
        if (action === 'plus') {
            servicio.cantidad++;
        } else if (action === 'minus' && servicio.cantidad > 1) {
            servicio.cantidad--;
        }
        
        actualizarFilaServicio(servicio);
        actualizarTotales();
        actualizarHiddenInputs();
        
        // Actualizar variable global y guardar estado
        window.exponerServiciosEnVenta(serviciosEnVenta);
        if (window.VentasValidacion) {
            window.VentasValidacion.guardarEstado();
        }
    });

    // Eliminar servicio
    $(document).on('click', '.btn-eliminar-servicio', function() {
        const fila = $(this).closest('tr');
        const servicioId = parseInt(fila.data('servicio-id'));
        const servicio = serviciosEnVenta.find(s => s.id === servicioId);
        
        // Remover del array
        serviciosEnVenta = serviciosEnVenta.filter(s => s.id !== servicioId);
        
        // Remover fila con animación
        fila.fadeOut(300, function() {
            $(this).remove();
            
            // Mostrar mensaje si no hay servicios
            if (serviciosEnVenta.length === 0) {
                $('#noServicios').show();
            }
        });
        
        // Toast de servicio eliminado
        if (window.toastr) {
            toastr.warning(`${servicio.nombre} eliminado`, 'Servicio eliminado', {
                timeOut: 2000
            });
        }
        
        actualizarTotales();
        actualizarHiddenInputs();
        
        // Actualizar variable global y guardar estado
        window.exponerServiciosEnVenta(serviciosEnVenta);
        if (window.VentasValidacion) {
            window.VentasValidacion.guardarEstado();
        }
    });

    // Actualizar totales cuando cambia el descuento
    $('#descuentoInput').on('input', function() {
        actualizarTotales();
        
        // Guardar estado
        if (window.VentasValidacion) {
            window.VentasValidacion.guardarEstado();
        }
    });

    // Actualizar totales
    function actualizarTotales() {
        const subtotal = serviciosEnVenta.reduce((sum, servicio) => sum + (servicio.precio * servicio.cantidad), 0);
        const descuento = parseFloat($('#descuentoInput').val()) || 0;
        const total = Math.max(0, subtotal - descuento);
        
        $('#subtotalDisplay').text(`$${subtotal.toFixed(2)}`);
        $('#totalDisplay').text(`$${total.toFixed(2)}`);
        
        // Habilitar/deshabilitar botón procesar venta
        $('#btnProcesarVenta').prop('disabled', serviciosEnVenta.length === 0);
    }

    // Actualizar hidden inputs para el formulario
    function actualizarHiddenInputs() {
        $('#hiddenInputsContainer').empty();
        
        serviciosEnVenta.forEach((servicio, index) => {
            $('#hiddenInputsContainer').append(`
                <input type="hidden" name="VentaActual.Detalles[${index}].ServicioId" value="${servicio.servicioId}">
                <input type="hidden" name="VentaActual.Detalles[${index}].NombreServicio" value="${servicio.nombre}">
                <input type="hidden" name="VentaActual.Detalles[${index}].PrecioUnitario" value="${servicio.precio}">
                <input type="hidden" name="VentaActual.Detalles[${index}].Cantidad" value="${servicio.cantidad}">
            `);
        });
    }

    // Limpiar venta
    $('#btnLimpiarVenta').click(function() {
        if (serviciosEnVenta.length === 0) {
            toastr.info('No hay nada que limpiar', 'Información');
            return;
        }
        
        if (confirm('¿Está seguro que desea limpiar la venta actual?')) {
            serviciosEnVenta = [];
            $('#serviciosVenta').empty();
            $('#noServicios').show();
            $('#descuentoInput').val(0);
            actualizarTotales();
            actualizarHiddenInputs();
            
            // Limpiar sessionStorage
            if (window.VentasValidacion) {
                sessionStorage.removeItem(window.VentasValidacion.STORAGE_KEY);
            }
            
            toastr.success('Venta limpiada correctamente', 'Limpieza exitosa');
        }
    });

    // BUSCADOR SERVICIOS
    $('#buscadorServicios').on('input', function() {
        const busqueda = $(this).val().toLowerCase();
        let hayResultados = false;
        
        if (busqueda === '') {
            // Mostrar todas las categorías y servicios
            $('.categoria-servicio').show();
            $('.servicio-item').show();
            $('#sinResultados').hide();
            return;
        }
        
        $('.categoria-servicio').each(function() {
            const categoria = $(this);
            let categoriaVisible = false;
            
            categoria.find('.servicio-item').each(function() {
                const servicio = $(this);
                const nombre = servicio.data('nombre');
                const tipo = servicio.data('tipo');
                
                if (nombre.includes(busqueda) || tipo.includes(busqueda)) {
                    servicio.show();
                    categoriaVisible = true;
                    hayResultados = true;
                } else {
                    servicio.hide();
                }
            });
            
            if (categoriaVisible) {
                categoria.show();
                // Expandir categoría si tiene resultados
                categoria.find('.collapse').addClass('show');
            } else {
                categoria.hide();
            }
        });
        
        $('#sinResultados').toggle(!hayResultados);
    });
    
    // Función para restaurar servicios desde sessionStorage
    window.restaurarServicios = function(serviciosGuardados) {
        if (!serviciosGuardados || !Array.isArray(serviciosGuardados)) return;
        
        serviciosGuardados.forEach(servicio => {
            // Buscar y hacer click en el botón del servicio
            const boton = $(`.btn-agregar-servicio[data-servicio-id="${servicio.servicioId}"]`);
            if (boton.length > 0) {
                // Agregar el servicio
                for (let i = 0; i < (servicio.cantidad || 1); i++) {
                    boton.click();
                }
            }
        });
        
        toastr.info('Servicios recuperados de la sesión anterior', 'Recuperación exitosa', {
            timeOut: 3000
        });
    };

    // Inicializar
    $('#noServicios').show();
    actualizarTotales();
    
    // Si hay datos guardados, recuperarlos
    if (window.VentasValidacion) {
        // Dar tiempo para que se cargue todo
        setTimeout(function() {
            window.VentasValidacion.recuperarDatosGuardados();
        }, 500);
    }
});
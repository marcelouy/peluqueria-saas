// wwwroot/js/pos-articulos.js
// NUEVO ARCHIVO - No modifica pos-funciones.js

$(document).ready(function() {
    let articulosEnVenta = [];
    
    // Cargar artículos al abrir tab
    $('#articulos-tab').on('shown.bs.tab', function() {
        cargarArticulosDisponibles();
    });
    
    // Cargar artículos disponibles
    function cargarArticulosDisponibles() {
        console.log('1. Iniciando carga de artículos...');
        
        $.get('/Articulos/GetArticulosParaVenta', function(articulos) {
            console.log('2. Artículos recibidos:', articulos);
            console.log('3. Container existe?', $('#articulosDisponibles').length);
            
            let html = '';
            articulos.forEach(art => {
                console.log('4. Procesando:', art.nombre);
                const stockClass = art.stock > 0 ? 'success' : 'danger';
                const disabled = art.stock <= 0 ? 'disabled' : '';
                
                html += `
                    <div class="col-md-4 col-sm-6 mb-3">
                        <div class="card articulo-card ${disabled}">
                            <div class="card-body">
                                <h6>${art.nombre}</h6>
                                <p class="text-muted small">${art.codigo || 'Sin código'}</p>
                                <div class="d-flex justify-content-between">
                                    <span class="fw-bold">$${art.precio}</span>
                                    <span class="badge bg-${stockClass}">Stock: ${art.stock}</span>
                                </div>
                                <button type="button" class="btn btn-sm btn-primary w-100 mt-2 btn-agregar-articulo" 
                                        data-articulo='${JSON.stringify(art)}' ${disabled}>
                                    <i class="fas fa-plus"></i> Agregar
                                </button>
                            </div>
                        </div>
                    </div>
                `;
            });
            
            console.log('5. HTML generado, longitud:', html.length);
            $('#articulosDisponibles').html(html);
            console.log('6. HTML insertado');
        });
    }

    // Buscador de artículos
    $('#buscadorArticulos').on('input', function() {
        const busqueda = $(this).val().toLowerCase();
        
        $('#articulosDisponibles .col-md-4').each(function() {
            const card = $(this);
            const nombre = card.find('h6').text().toLowerCase();
            const codigo = card.find('.text-muted').text().toLowerCase();
            
            if (nombre.includes(busqueda) || codigo.includes(busqueda)) {
                card.show();
            } else {
                card.hide();
            }
        });
        
        // Mostrar mensaje si no hay resultados
        const visibles = $('#articulosDisponibles .col-md-4:visible').length;
        if (visibles === 0 && busqueda !== '') {
            if ($('#sinResultadosArticulos').length === 0) {
                $('#articulosDisponibles').append(`
                    <div id="sinResultadosArticulos" class="col-12 text-center py-4">
                        <i class="fas fa-search fa-2x text-muted mb-2"></i>
                        <p class="text-muted">No se encontraron artículos</p>
                    </div>
                `);
            }
            $('#sinResultadosArticulos').show();
        } else {
            $('#sinResultadosArticulos').hide();
        }
    });

    // Llamar manualmente para probar
    cargarArticulosDisponibles();
    
    // Agregar artículo al carrito
    $(document).on('click', '.btn-agregar-articulo', function(e) {
        e.preventDefault(); // Prevenir submit
        e.stopPropagation(); // Detener propagación
        
        const articulo = JSON.parse($(this).attr('data-articulo'));
        
        // Verificar stock
        if (articulo.stock <= 0) {
            toastr.warning('Sin stock disponible');
            return;
        }
        
        agregarArticuloATabla(articulo);
    });
    
    function agregarArticuloATabla(articulo) {
        // Contar TODOS los detalles existentes (servicios + artículos)
        const detallesExistentes = $('#hiddenInputsContainer .detalle-inputs').length;
        const index = detallesExistentes;
        
        // Agregar fila visual a la tabla
        const fila = `
            <tr data-tipo="articulo" data-articulo-id="${articulo.id}" data-index="${index}">
                <td>
                    <strong>${articulo.nombre}</strong><br>
                    <small class="text-info"><i class="fas fa-box"></i> Artículo</small>
                </td>
                <td>$${articulo.precio}</td>
                <td>
                    <input type="number" class="form-control form-control-sm cantidad-articulo" 
                        value="1" min="1" max="${articulo.stock}" style="width: 70px;"
                        data-index="${index}" data-precio="${articulo.precio}">
                </td>
                <td class="subtotal">$${articulo.precio}</td>
                <td>
                    <button type="button" class="btn btn-sm btn-danger btn-eliminar-articulo"
                            data-index="${index}">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
        
        // Hidden inputs con la estructura correcta
        const hiddenInputs = `
            <div class="detalle-inputs articulo-inputs" data-index="${index}">
                <input type="hidden" name="VentaActual.Detalles[${index}].TipoItem" value="ARTICULO" />
                <input type="hidden" name="VentaActual.Detalles[${index}].ArticuloId" value="${articulo.id}" />
                <input type="hidden" name="VentaActual.Detalles[${index}].ServicioId" value="1" />
                <input type="hidden" name="VentaActual.Detalles[${index}].NombreServicio" value="${articulo.nombre}" />
                <input type="hidden" name="VentaActual.Detalles[${index}].PrecioUnitario" value="${articulo.precio}" />
                <input type="hidden" name="VentaActual.Detalles[${index}].Cantidad" value="1" class="cantidad-hidden" />
                <input type="hidden" name="VentaActual.Detalles[${index}].Subtotal" value="${articulo.precio}" class="subtotal-hidden" />
                <input type="hidden" name="VentaActual.Detalles[${index}].EmpleadoServicioId" value="" />
                <input type="hidden" name="VentaActual.Detalles[${index}].NotasServicio" value="" />
            </div>
        `;
        
        // Agregar a la tabla y al contenedor de hiddens
        $('#serviciosVenta').append(fila);
        $('#hiddenInputsContainer').append(hiddenInputs);
        $('#noServicios').hide();
        
        // Actualizar totales
        if (typeof actualizarTotales === 'function') {
            actualizarTotales();
        }
        
        toastr.success(`${articulo.nombre} agregado`);
    }

    // Función para reindexar todos los inputs cuando se elimina un item
    function reindexarDetalles() {
        let index = 0;
        $('#serviciosVenta tr').each(function() {
            const $row = $(this);
            const oldIndex = $row.data('index');
            
            // Actualizar índice en la fila
            $row.attr('data-index', index);
            $row.find('.cantidad-servicio, .cantidad-articulo').attr('data-index', index);
            $row.find('.btn-eliminar-servicio, .btn-eliminar-articulo').attr('data-index', index);
            
            // Actualizar hidden inputs
            const $hiddenDiv = $(`.detalle-inputs[data-index="${oldIndex}"]`);
            if ($hiddenDiv.length) {
                $hiddenDiv.attr('data-index', index);
                $hiddenDiv.find('input').each(function() {
                    const name = $(this).attr('name');
                    if (name) {
                        const newName = name.replace(/\[\d+\]/, `[${index}]`);
                        $(this).attr('name', newName);
                    }
                });
            }
            
            index++;
        });
    }

    // Modificar el eliminar artículo para reindexar
    $(document).on('click', '.btn-eliminar-articulo', function() {
        const index = $(this).data('index');
        
        // Eliminar fila y hidden inputs
        $(this).closest('tr').remove();
        $(`.articulo-inputs[data-index="${index}"]`).remove();
        
        // Reindexar todos los detalles
        reindexarDetalles();
        
        // Actualizar totales
        if (typeof actualizarTotales === 'function') {
            actualizarTotales();
        }
        
        // Mostrar mensaje si no hay items
        if ($('#serviciosVenta tr').length === 0) {
            $('#noServicios').show();
        }
    });


    // Actualizar cantidad
    $(document).on('change', '.cantidad-articulo', function() {
        const index = $(this).data('index');
        const cantidad = parseInt($(this).val());
        const precio = parseFloat($(this).data('precio'));
        const subtotal = precio * cantidad;
        
        $(this).closest('tr').find('.subtotal').text(`$${subtotal.toFixed(2)}`);
        $(`.articulo-inputs[data-index="${index}"] .cantidad-hidden`).val(cantidad);
        
        if (typeof actualizarTotales === 'function') {
            actualizarTotales();
        }
    });

    // Eliminar artículo
    $(document).on('click', '.btn-eliminar-articulo', function() {
        const index = $(this).data('index');
        $(this).closest('tr').remove();
        $(`.articulo-inputs[data-index="${index}"]`).remove();
        
        if (typeof actualizarTotales === 'function') {
            actualizarTotales();
        }
        
        if ($('#serviciosVenta tr').length === 0) {
            $('#noServicios').show();
        }
    });
});
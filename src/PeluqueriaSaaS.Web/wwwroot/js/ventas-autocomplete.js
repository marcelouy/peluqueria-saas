// ========================================
// AUTOCOMPLETE INTELIGENTE PARA VENTAS
// Select2 para empleados y clientes
// Ubicación: wwwroot/js/ventas-autocomplete.js
// ========================================

(function() {
    'use strict';
    
    window.VentasAutocomplete = {
        
        // Inicializar Select2 para empleados y clientes
        init: function() {
            this.configurarEmpleados();
            this.configurarClientes();
            this.agregarBotonesRapidos();
            this.configurarNuevoClienteRapido();
        },
        
        // Configurar Select2 para empleados
        configurarEmpleados: function() {
            const empleadoSelect = $('#VentaActual_EmpleadoId');
            
            if (empleadoSelect.length === 0) return;
            
            empleadoSelect.select2({
                placeholder: '🔍 Buscar empleado por nombre o cargo...',
                minimumInputLength: 0, // Mostrar todos al hacer clic
                width: '100%',
                language: {
                    inputTooShort: function() {
                        return 'Haga clic para ver todos o escriba para buscar...';
                    },
                    noResults: function() {
                        return 'No se encontraron empleados';
                    },
                    searching: function() {
                        return 'Buscando...';
                    }
                },
                ajax: {
                    url: '/Ventas/BuscarEmpleados',
                    dataType: 'json',
                    delay: 250,
                    data: function(params) {
                        return {
                            q: params.term || '',
                            page: params.page || 1
                        };
                    },
                    processResults: function(data, params) {
                        params.page = params.page || 1;
                        
                        return {
                            results: data.items.map(function(empleado) {
                                return {
                                    id: empleado.empleadoId || empleado.id,
                                    text: empleado.nombre + ' ' + empleado.apellido,
                                    cargo: empleado.cargo,
                                    telefono: empleado.telefono,
                                    activo: empleado.esActivo
                                };
                            }),
                            pagination: {
                                more: data.hasMore
                            }
                        };
                    },
                    cache: true
                },
                templateResult: this.formatEmpleado,
                templateSelection: this.formatEmpleadoSelection,
                // Permitir limpiar selección
                allowClear: true
            });
            
            // Abrir automáticamente al hacer focus
            empleadoSelect.on('select2:open', function() {
                setTimeout(function() {
                    $('.select2-search__field').focus();
                }, 100);
            });
        },
        
        // Configurar Select2 para clientes
        configurarClientes: function() {
            const clienteSelect = $('#VentaActual_ClienteId');
            
            if (clienteSelect.length === 0) return;
            
            clienteSelect.select2({
                placeholder: '🔍 Buscar cliente por nombre, teléfono o email...',
                minimumInputLength: 0,
                width: '100%',
                language: {
                    inputTooShort: function() {
                        return 'Haga clic para ver todos o escriba para buscar...';
                    },
                    noResults: function() {
                        return 'No se encontraron clientes';
                    },
                    searching: function() {
                        return 'Buscando...';
                    }
                },
                ajax: {
                    url: '/Ventas/BuscarClientes',
                    dataType: 'json',
                    delay: 250,
                    data: function(params) {
                        return {
                            q: params.term || '',
                            page: params.page || 1
                        };
                    },
                    processResults: function(data, params) {
                        params.page = params.page || 1;
                        
                        // Agregar opción de nuevo cliente al inicio si hay búsqueda
                        var results = data.items.map(function(cliente) {
                            return {
                                id: cliente.clienteId || cliente.id,
                                text: cliente.nombre + ' ' + cliente.apellido,
                                telefono: cliente.telefono,
                                email: cliente.email,
                                ultimaVisita: cliente.ultimaVisita,
                                totalVisitas: cliente.totalVisitas || 0
                            };
                        });
                        
                        // Si hay término de búsqueda, agregar opción de crear nuevo
                        if (params.term && params.term.length > 2) {
                            results.unshift({
                                id: 'nuevo',
                                text: '➕ Crear nuevo cliente: ' + params.term,
                                isNuevo: true,
                                nombre: params.term
                            });
                        }
                        
                        return {
                            results: results,
                            pagination: {
                                more: data.hasMore
                            }
                        };
                    },
                    cache: true
                },
                templateResult: this.formatCliente,
                templateSelection: this.formatClienteSelection,
                allowClear: true,
                // Manejar tamaño para tablet
                dropdownCssClass: 'select2-dropdown-tablet'
            });
            
            // Manejar selección de nuevo cliente
            clienteSelect.on('select2:select', function(e) {
                if (e.params.data.isNuevo) {
                    VentasAutocomplete.abrirModalNuevoCliente(e.params.data.nombre);
                    // Limpiar selección
                    $(this).val(null).trigger('change');
                }
            });
            
            // Abrir automáticamente al hacer focus
            clienteSelect.on('select2:open', function() {
                setTimeout(function() {
                    $('.select2-search__field').focus();
                }, 100);
            });
        },
        
        // Formato para mostrar empleados en el dropdown
        formatEmpleado: function(empleado) {
            if (!empleado.id) return empleado.text;
            
            var $container = $(
                '<div class="select2-empleado-result">' +
                    '<div class="empleado-info">' +
                        '<span class="empleado-nombre">' + empleado.text + '</span>' +
                        '<span class="badge bg-secondary ms-2">' + (empleado.cargo || 'Empleado') + '</span>' +
                    '</div>' +
                    (empleado.telefono ? '<small class="text-muted">📱 ' + empleado.telefono + '</small>' : '') +
                '</div>'
            );
            
            return $container;
        },
        
        // Formato para empleado seleccionado
        formatEmpleadoSelection: function(empleado) {
            return empleado.text || empleado.id;
        },
        
        // Formato para mostrar clientes en el dropdown
        formatCliente: function(cliente) {
            if (!cliente.id) return cliente.text;
            
            // Si es opción de nuevo cliente
            if (cliente.isNuevo) {
                return $(
                    '<div class="select2-nuevo-cliente">' +
                        '<strong style="color: #28a745;">' + cliente.text + '</strong>' +
                    '</div>'
                );
            }
            
            var visitasInfo = '';
            if (cliente.totalVisitas > 0) {
                visitasInfo = '<span class="badge bg-info ms-2">' + cliente.totalVisitas + ' visitas</span>';
            }
            
            var $container = $(
                '<div class="select2-cliente-result">' +
                    '<div class="cliente-info">' +
                        '<span class="cliente-nombre">' + cliente.text + '</span>' +
                        visitasInfo +
                    '</div>' +
                    '<div class="cliente-contacto">' +
                        (cliente.telefono ? '<small class="text-muted">📱 ' + cliente.telefono + '</small>' : '') +
                        (cliente.email ? '<small class="text-muted ms-2">📧 ' + cliente.email + '</small>' : '') +
                    '</div>' +
                '</div>'
            );
            
            return $container;
        },
        
        // Formato para cliente seleccionado
        formatClienteSelection: function(cliente) {
            if (cliente.isNuevo) return '';
            return cliente.text || cliente.id;
        },
        
        // Agregar botones de acceso rápido
        agregarBotonesRapidos: function() {
            // Agregar sección de clientes frecuentes después del select de cliente
            const clienteContainer = $('#VentaActual_ClienteId').parent();
            
            if (clienteContainer.length > 0 && $('#clientesFrecuentes').length === 0) {
                const botonesHtml = `
                    <div id="clientesFrecuentes" class="mt-2" style="display: none;">
                        <small class="text-muted d-block mb-1">Clientes frecuentes:</small>
                        <div class="clientes-rapidos"></div>
                    </div>
                `;
                
                clienteContainer.append(botonesHtml);
                
                // Cargar clientes frecuentes
                this.cargarClientesFrecuentes();
            }
            
            // Agregar botón de nuevo cliente si no existe
            if ($('#btnNuevoClienteRapido').length === 0) {
                const btnNuevoCliente = `
                    <button type="button" id="btnNuevoClienteRapido" class="btn btn-outline-success btn-sm mt-2">
                        <i class="fas fa-user-plus"></i> Nuevo Cliente Rápido
                    </button>
                `;
                clienteContainer.append(btnNuevoCliente);
            }
        },
        
        // Cargar clientes frecuentes
        cargarClientesFrecuentes: function() {
            $.get('/Ventas/ClientesFrecuentes', function(clientes) {
                if (clientes && clientes.length > 0) {
                    var botonesHtml = '';
                    
                    clientes.slice(0, 5).forEach(function(cliente) {
                        botonesHtml += `
                            <button type="button" 
                                    class="btn btn-outline-primary btn-sm me-1 mb-1 btn-cliente-frecuente"
                                    data-cliente-id="${cliente.clienteId}"
                                    data-cliente-nombre="${cliente.nombre} ${cliente.apellido}">
                                <i class="fas fa-user"></i> ${cliente.nombre} 
                                <span class="badge bg-secondary">${cliente.visitas}</span>
                            </button>
                        `;
                    });
                    
                    $('.clientes-rapidos').html(botonesHtml);
                    $('#clientesFrecuentes').show();
                    
                    // Manejar clic en cliente frecuente
                    $('.btn-cliente-frecuente').on('click', function() {
                        var clienteId = $(this).data('cliente-id');
                        var clienteNombre = $(this).data('cliente-nombre');
                        
                        // Crear opción y seleccionar en Select2
                        var newOption = new Option(clienteNombre, clienteId, true, true);
                        $('#VentaActual_ClienteId').append(newOption).trigger('change');
                        
                        // Feedback visual
                        $(this).removeClass('btn-outline-primary').addClass('btn-primary');
                        setTimeout(() => {
                            $(this).removeClass('btn-primary').addClass('btn-outline-primary');
                        }, 1000);
                    });
                }
            });
        },
        
        // Configurar modal de nuevo cliente rápido
        configurarNuevoClienteRapido: function() {
            // Manejar clic en botón de nuevo cliente
            $(document).on('click', '#btnNuevoClienteRapido', function() {
                VentasAutocomplete.abrirModalNuevoCliente('');
            });
        },
        
        // Abrir modal para crear nuevo cliente
        abrirModalNuevoCliente: function(nombreInicial) {
            // Si no existe el modal, crearlo
            if ($('#modalNuevoClienteRapido').length === 0) {
                this.crearModalNuevoCliente();
            }
            
            // Limpiar y prellenar nombre si viene de búsqueda
            $('#nuevoClienteNombre').val(nombreInicial || '');
            $('#nuevoClienteApellido').val('');
            $('#nuevoClienteTelefono').val('');
            $('#nuevoClienteEmail').val('');
            
            // Mostrar modal
            var modal = new bootstrap.Modal(document.getElementById('modalNuevoClienteRapido'));
            modal.show();
            
            // Focus en primer campo vacío
            setTimeout(function() {
                if (nombreInicial) {
                    $('#nuevoClienteApellido').focus();
                } else {
                    $('#nuevoClienteNombre').focus();
                }
            }, 500);
        },
        
        // Crear modal HTML para nuevo cliente
        crearModalNuevoCliente: function() {
            const modalHtml = `
                <div class="modal fade" id="modalNuevoClienteRapido" tabindex="-1">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header bg-success text-white">
                                <h5 class="modal-title">
                                    <i class="fas fa-user-plus"></i> Nuevo Cliente Rápido
                                </h5>
                                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                            </div>
                            <div class="modal-body">
                                <form id="formNuevoClienteRapido">
                                    <div class="row">
                                        <div class="col-md-6 mb-3">
                                            <label class="form-label">Nombre <span class="text-danger">*</span></label>
                                            <input type="text" id="nuevoClienteNombre" class="form-control" required>
                                        </div>
                                        <div class="col-md-6 mb-3">
                                            <label class="form-label">Apellido <span class="text-danger">*</span></label>
                                            <input type="text" id="nuevoClienteApellido" class="form-control" required>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col-md-6 mb-3">
                                            <label class="form-label">Teléfono <span class="text-danger">*</span></label>
                                            <input type="tel" id="nuevoClienteTelefono" class="form-control" 
                                                   placeholder="099 123 456" required>
                                        </div>
                                        <div class="col-md-6 mb-3">
                                            <label class="form-label">Email</label>
                                            <input type="email" id="nuevoClienteEmail" class="form-control" 
                                                   placeholder="cliente@email.com">
                                        </div>
                                    </div>
                                </form>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                                    <i class="fas fa-times"></i> Cancelar
                                </button>
                                <button type="button" class="btn btn-success" id="btnGuardarClienteRapido">
                                    <i class="fas fa-save"></i> Guardar y Seleccionar
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            $('body').append(modalHtml);
            
            // Manejar guardado
            $('#btnGuardarClienteRapido').on('click', function() {
                VentasAutocomplete.guardarNuevoCliente();
            });
            
            // Enter para guardar
            $('#formNuevoClienteRapido').on('submit', function(e) {
                e.preventDefault();
                VentasAutocomplete.guardarNuevoCliente();
            });
        },
        
        // Guardar nuevo cliente
        guardarNuevoCliente: function() {
            const nombre = $('#nuevoClienteNombre').val().trim();
            const apellido = $('#nuevoClienteApellido').val().trim();
            const telefono = $('#nuevoClienteTelefono').val().trim();
            const email = $('#nuevoClienteEmail').val().trim();
            
            // Validación básica
            if (!nombre || !apellido || !telefono) {
                toastr.warning('Complete los campos obligatorios', 'Validación');
                return;
            }
            
            // Deshabilitar botón mientras guarda
            $('#btnGuardarClienteRapido').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Guardando...');
            
            // Guardar via AJAX
            $.ajax({
                url: '/Ventas/CrearClienteRapido',
                type: 'POST',
                data: {
                    nombre: nombre,
                    apellido: apellido,
                    telefono: telefono,
                    email: email
                },
                success: function(response) {
                    if (response.success) {
                        // Cerrar modal
                        var modal = bootstrap.Modal.getInstance(document.getElementById('modalNuevoClienteRapido'));
                        modal.hide();
                        
                        // Crear opción y seleccionar en Select2
                        var nombreCompleto = nombre + ' ' + apellido;
                        var newOption = new Option(nombreCompleto, response.clienteId, true, true);
                        $('#VentaActual_ClienteId').append(newOption).trigger('change');
                        
                        // Mostrar éxito
                        toastr.success('Cliente creado y seleccionado', 'Éxito');
                        
                        // Actualizar clientes frecuentes
                        VentasAutocomplete.cargarClientesFrecuentes();
                    } else {
                        toastr.error(response.message || 'Error al crear cliente', 'Error');
                    }
                },
                error: function() {
                    toastr.error('Error al crear cliente', 'Error');
                },
                complete: function() {
                    $('#btnGuardarClienteRapido').prop('disabled', false).html('<i class="fas fa-save"></i> Guardar y Seleccionar');
                }
            });
        }
    };
    
    // Inicializar cuando el DOM esté listo
    $(document).ready(function() {
        // Verificar que Select2 esté cargado
        if (typeof $.fn.select2 === 'undefined') {
            console.error('Select2 no está cargado. Asegúrese de incluir la librería.');
            return;
        }
        
        VentasAutocomplete.init();
    });
    
})();
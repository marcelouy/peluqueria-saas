// ========================================
// SELECCIÓN VISUAL SIN TECLADO
// Optimizado para tablet/touch
// Ubicación: wwwroot/js/ventas-seleccion-visual.js
// ========================================

(function() {
    'use strict';
    
    window.VentasSeleccionVisual = {
        
        empleadoSeleccionado: null,
        clienteSeleccionado: null,
        
        init: function() {
            this.convertirEmpleadosABotones();
            this.convertirClientesABotones();
            this.configurarFiltrosRapidos();
        },
        
        // Convertir dropdown de empleados a grid visual
        convertirEmpleadosABotones: function() {
            const selectEmpleado = document.getElementById('VentaActual_EmpleadoId');
            if (!selectEmpleado) return;
            
            // Ocultar select original
            selectEmpleado.style.display = 'none';
            
            // Crear contenedor de botones
            const container = document.createElement('div');
            container.className = 'empleados-grid-container mb-3';
            container.innerHTML = `
                <div class="empleado-seleccionado-display alert alert-info mb-2" style="display: none;">
                    <strong>Empleado:</strong> <span id="empleadoSeleccionadoNombre">Ninguno</span>
                    <button type="button" class="btn btn-sm btn-warning float-end" onclick="VentasSeleccionVisual.cambiarEmpleado()">
                        <i class="fas fa-exchange-alt"></i> Cambiar
                    </button>
                </div>
                
                <div class="empleados-grid" id="empleadosGrid">
                    <div class="text-center py-4">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Cargando empleados...</span>
                        </div>
                    </div>
                </div>
            `;
            
            selectEmpleado.parentNode.insertBefore(container, selectEmpleado.nextSibling);
            
            // Cargar empleados como botones
            this.cargarEmpleadosBotones();
        },
        
        cargarEmpleadosBotones: function() {
            const gridContainer = document.getElementById('empleadosGrid');
            const selectEmpleado = document.getElementById('VentaActual_EmpleadoId');
            
            // Obtener empleados del select
            const empleados = Array.from(selectEmpleado.options)
                .filter(opt => opt.value) // Excluir placeholder
                .map(opt => ({
                    id: opt.value,
                    nombre: opt.text,
                    iniciales: this.obtenerIniciales(opt.text)
                }));
            
            // Agrupar por inicial del nombre (opcional)
            const empleadosOrdenados = empleados.sort((a, b) => a.nombre.localeCompare(b.nombre));
            
            // Crear HTML de botones
            let html = `
                <div class="row g-2">
                    <div class="col-12">
                        <div class="btn-group-filtros mb-3" role="group">
                            <button type="button" class="btn btn-outline-secondary btn-sm" onclick="VentasSeleccionVisual.filtrarEmpleados('todos')">
                                Todos (${empleados.length})
                            </button>
                            <button type="button" class="btn btn-outline-primary btn-sm" onclick="VentasSeleccionVisual.filtrarEmpleados('estilista')">
                                Estilistas
                            </button>
                            <button type="button" class="btn btn-outline-info btn-sm" onclick="VentasSeleccionVisual.filtrarEmpleados('colorista')">
                                Coloristas
                            </button>
                            <button type="button" class="btn btn-outline-success btn-sm" onclick="VentasSeleccionVisual.filtrarEmpleados('barbero')">
                                Barberos
                            </button>
                        </div>
                    </div>
                </div>
                <div class="empleados-botones-grid">
            `;
            
            empleadosOrdenados.forEach(emp => {
                // Determinar color según cargo (si está en el nombre)
                let colorClass = 'btn-outline-primary';
                const nombreLower = emp.nombre.toLowerCase();
                if (nombreLower.includes('colorista')) colorClass = 'btn-outline-info';
                else if (nombreLower.includes('barbero')) colorClass = 'btn-outline-success';
                else if (nombreLower.includes('manicurista')) colorClass = 'btn-outline-warning';
                
                html += `
                    <button type="button" 
                            class="btn ${colorClass} btn-empleado" 
                            data-empleado-id="${emp.id}"
                            data-empleado-nombre="${emp.nombre}"
                            onclick="VentasSeleccionVisual.seleccionarEmpleado('${emp.id}', '${emp.nombre.replace(/'/g, "\\'")}')">
                        <div class="empleado-btn-content">
                            <div class="empleado-avatar">${emp.iniciales}</div>
                            <div class="empleado-nombre">${emp.nombre}</div>
                        </div>
                    </button>
                `;
            });
            
            html += '</div>';
            gridContainer.innerHTML = html;
        },
        
        // Convertir dropdown de clientes a sistema visual
        convertirClientesABotones: function() {
            const selectCliente = document.getElementById('VentaActual_ClienteId');
            if (!selectCliente) return;
            
            // Ocultar select original
            selectCliente.style.display = 'none';
            
            // Crear contenedor mejorado
            const container = document.createElement('div');
            container.className = 'clientes-container mb-3';
            container.innerHTML = `
                <div class="cliente-seleccionado-display alert alert-success mb-2" style="display: none;">
                    <strong>Cliente:</strong> <span id="clienteSeleccionadoNombre">Ninguno</span>
                    <button type="button" class="btn btn-sm btn-warning float-end" onclick="VentasSeleccionVisual.cambiarCliente()">
                        <i class="fas fa-exchange-alt"></i> Cambiar
                    </button>
                </div>
                
                <div class="clientes-opciones" id="clientesOpciones">
                    <!-- Tabs para organizar -->
                    <ul class="nav nav-tabs mb-3" role="tablist">
                        <li class="nav-item" role="presentation">
                            <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#clientesFrecuentes" type="button">
                                <i class="fas fa-star"></i> Frecuentes
                            </button>
                        </li>
                        <li class="nav-item" role="presentation">
                            <button class="nav-link" data-bs-toggle="tab" data-bs-target="#clientesTodos" type="button">
                                <i class="fas fa-users"></i> Todos
                            </button>
                        </li>
                        <li class="nav-item" role="presentation">
                            <button class="nav-link" data-bs-toggle="tab" data-bs-target="#clienteNuevo" type="button">
                                <i class="fas fa-user-plus"></i> Nuevo
                            </button>
                        </li>
                    </ul>
                    
                    <div class="tab-content">
                        <!-- Clientes Frecuentes -->
                        <div class="tab-pane fade show active" id="clientesFrecuentes">
                            <div class="row g-2" id="gridClientesFrecuentes">
                                <div class="text-center py-3">
                                    <i class="fas fa-spinner fa-spin"></i> Cargando...
                                </div>
                            </div>
                        </div>
                        
                        <!-- Todos los Clientes -->
                        <div class="tab-pane fade" id="clientesTodos">
                            <div class="clientes-lista" id="gridClientesTodos">
                                <!-- Se carga dinámicamente -->
                            </div>
                        </div>
                        
                        <!-- Nuevo Cliente -->
                        <div class="tab-pane fade" id="clienteNuevo">
                            <div class="card">
                                <div class="card-body">
                                    <h6>Crear Cliente Rápido</h6>
                                    <div class="row g-2">
                                        <div class="col-md-6">
                                            <input type="text" id="nuevoClienteNombre" class="form-control" 
                                                   placeholder="Nombre" autocomplete="off">
                                        </div>
                                        <div class="col-md-6">
                                            <input type="text" id="nuevoClienteApellido" class="form-control" 
                                                   placeholder="Apellido" autocomplete="off">
                                        </div>
                                        <div class="col-md-6">
                                            <input type="tel" id="nuevoClienteTelefono" class="form-control" 
                                                   placeholder="Teléfono" autocomplete="off">
                                        </div>
                                        <div class="col-md-6">
                                            <button type="button" class="btn btn-success w-100" 
                                                    onclick="VentasSeleccionVisual.crearClienteRapido()">
                                                <i class="fas fa-save"></i> Crear y Seleccionar
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            selectCliente.parentNode.insertBefore(container, selectCliente.nextSibling);
            
            // Cargar datos
            this.cargarClientesFrecuentes();
            this.cargarTodosLosClientes();
        },
        
        // Cargar clientes frecuentes
        cargarClientesFrecuentes: function() {
            // Simular carga de frecuentes (en producción vendría del servidor)
            const selectCliente = document.getElementById('VentaActual_ClienteId');
            const clientes = Array.from(selectCliente.options)
                .filter(opt => opt.value)
                .slice(0, 10) // Top 10
                .map(opt => ({
                    id: opt.value,
                    nombre: opt.text
                }));
            
            let html = '';
            clientes.forEach(cliente => {
                html += `
                    <div class="col-6 col-md-4">
                        <button type="button" class="btn btn-outline-primary w-100 btn-cliente-frecuente"
                                onclick="VentasSeleccionVisual.seleccionarCliente('${cliente.id}', '${cliente.nombre.replace(/'/g, "\\'")}')">
                            <i class="fas fa-user"></i><br>
                            ${cliente.nombre}
                        </button>
                    </div>
                `;
            });
            
            document.getElementById('gridClientesFrecuentes').innerHTML = html || 
                '<div class="col-12 text-center text-muted">No hay clientes frecuentes aún</div>';
        },
        
        // Cargar todos los clientes
        cargarTodosLosClientes: function() {
            const selectCliente = document.getElementById('VentaActual_ClienteId');
            const clientes = Array.from(selectCliente.options)
                .filter(opt => opt.value)
                .map(opt => ({
                    id: opt.value,
                    nombre: opt.text
                }));
            
            // Agrupar por inicial para mejor organización
            const grupos = {};
            clientes.forEach(cliente => {
                const inicial = cliente.nombre[0].toUpperCase();
                if (!grupos[inicial]) grupos[inicial] = [];
                grupos[inicial].push(cliente);
            });
            
            let html = '<div class="accordion" id="accordionClientes">';
            
            Object.keys(grupos).sort().forEach((letra, index) => {
                html += `
                    <div class="accordion-item">
                        <h2 class="accordion-header">
                            <button class="accordion-button ${index > 0 ? 'collapsed' : ''}" type="button" 
                                    data-bs-toggle="collapse" data-bs-target="#collapse${letra}">
                                ${letra} <span class="badge bg-secondary ms-2">${grupos[letra].length}</span>
                            </button>
                        </h2>
                        <div id="collapse${letra}" class="accordion-collapse collapse ${index === 0 ? 'show' : ''}"
                             data-bs-parent="#accordionClientes">
                            <div class="accordion-body">
                                <div class="list-group">
                `;
                
                grupos[letra].forEach(cliente => {
                    html += `
                        <button type="button" class="list-group-item list-group-item-action"
                                onclick="VentasSeleccionVisual.seleccionarCliente('${cliente.id}', '${cliente.nombre.replace(/'/g, "\\'")}')">
                            <i class="fas fa-user text-primary"></i> ${cliente.nombre}
                        </button>
                    `;
                });
                
                html += `
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            });
            
            html += '</div>';
            document.getElementById('gridClientesTodos').innerHTML = html;
        },
        
        // Seleccionar empleado
        seleccionarEmpleado: function(id, nombre) {
            // Actualizar select original
            document.getElementById('VentaActual_EmpleadoId').value = id;
            
            // Actualizar UI
            document.getElementById('empleadoSeleccionadoNombre').textContent = nombre;
            document.querySelector('.empleado-seleccionado-display').style.display = 'block';
            document.getElementById('empleadosGrid').style.display = 'none';
            
            this.empleadoSeleccionado = { id, nombre };
            
            // Feedback táctil
            if (window.navigator && window.navigator.vibrate) {
                window.navigator.vibrate(50);
            }
            
            // Toast de confirmación
            if (window.toastr) {
                toastr.success(`Empleado seleccionado: ${nombre}`, '', { timeOut: 1000 });
            }
        },
        
        // Cambiar empleado
        cambiarEmpleado: function() {
            document.querySelector('.empleado-seleccionado-display').style.display = 'none';
            document.getElementById('empleadosGrid').style.display = 'block';
        },
        
        // Seleccionar cliente
        seleccionarCliente: function(id, nombre) {
            document.getElementById('VentaActual_ClienteId').value = id;
            document.getElementById('clienteSeleccionadoNombre').textContent = nombre;
            document.querySelector('.cliente-seleccionado-display').style.display = 'block';
            document.getElementById('clientesOpciones').style.display = 'none';
            
            this.clienteSeleccionado = { id, nombre };
            
            if (window.navigator && window.navigator.vibrate) {
                window.navigator.vibrate(50);
            }
            
            if (window.toastr) {
                toastr.success(`Cliente seleccionado: ${nombre}`, '', { timeOut: 1000 });
            }
        },
        
        // Cambiar cliente
        cambiarCliente: function() {
            document.querySelector('.cliente-seleccionado-display').style.display = 'none';
            document.getElementById('clientesOpciones').style.display = 'block';
        },
        
        // Crear cliente rápido
        crearClienteRapido: function() {
            const nombre = document.getElementById('nuevoClienteNombre').value.trim();
            const apellido = document.getElementById('nuevoClienteApellido').value.trim();
            const telefono = document.getElementById('nuevoClienteTelefono').value.trim();
            
            if (!nombre || !apellido || !telefono) {
                toastr.warning('Complete todos los campos', 'Validación');
                return;
            }
            
            // Aquí iría el AJAX para crear el cliente
            // Por ahora simulamos
            const nuevoId = 'nuevo_' + Date.now();
            const nombreCompleto = `${nombre} ${apellido}`;
            
            // Agregar al select original
            const selectCliente = document.getElementById('VentaActual_ClienteId');
            const option = document.createElement('option');
            option.value = nuevoId;
            option.text = nombreCompleto;
            selectCliente.add(option);
            
            // Seleccionar automáticamente
            this.seleccionarCliente(nuevoId, nombreCompleto);
            
            // Limpiar formulario
            document.getElementById('nuevoClienteNombre').value = '';
            document.getElementById('nuevoClienteApellido').value = '';
            document.getElementById('nuevoClienteTelefono').value = '';
            
            // Cambiar a tab de frecuentes
            document.querySelector('[data-bs-target="#clientesFrecuentes"]').click();
            
            toastr.success('Cliente creado y seleccionado', 'Éxito');
        },
        
        // Filtrar empleados por cargo
        filtrarEmpleados: function(tipo) {
            const botones = document.querySelectorAll('.btn-empleado');
            
            botones.forEach(btn => {
                const nombre = btn.dataset.empleadoNombre.toLowerCase();
                
                if (tipo === 'todos') {
                    btn.style.display = '';
                } else {
                    if (nombre.includes(tipo)) {
                        btn.style.display = '';
                    } else {
                        btn.style.display = 'none';
                    }
                }
            });
        },
        
        // Obtener iniciales
        obtenerIniciales: function(nombre) {
            const partes = nombre.split(' ');
            if (partes.length >= 2) {
                return partes[0][0] + partes[1][0];
            }
            return nombre.substring(0, 2).toUpperCase();
        }
    };
    
    // Inicializar
    document.addEventListener('DOMContentLoaded', function() {
        VentasSeleccionVisual.init();
    });
    
})();
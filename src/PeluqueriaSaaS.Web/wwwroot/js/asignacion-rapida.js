// asignacion-rapida.js - Manejo de asignación rápida de servicios
let servicioIndex = 1;

document.addEventListener('DOMContentLoaded', function() {
    // Botón agregar servicio
    const btnAdd = document.getElementById('add-servicio');
    if (btnAdd) {
        btnAdd.addEventListener('click', agregarServicio);
    }
    
    // Validación del formulario
    const form = document.getElementById('formAsignacion');
    if (form) {
        form.addEventListener('submit', validarAsignacion);
    }
});

function agregarServicio() {
    const container = document.getElementById('servicios-container');
    const template = document.querySelector('.servicio-row');
    
    if (!template || !container) return;
    
    const newRow = template.cloneNode(true);
    
    // Actualizar names con nuevo índice
    const selects = newRow.querySelectorAll('select');
    if (selects[0]) selects[0].name = `Asignaciones[${servicioIndex}].ServicioId`;
    if (selects[1]) selects[1].name = `Asignaciones[${servicioIndex}].EmpleadoAsignadoId`;
    
    // Resetear valores
    selects.forEach(s => s.value = '0');
    
    // Configurar botón eliminar
    const btnRemove = newRow.querySelector('.remove-servicio');
    if (btnRemove) {
        btnRemove.style.display = 'inline-block';
        btnRemove.onclick = function() {
            newRow.remove();
            actualizarIndices();
        };
    }
    
    container.appendChild(newRow);
    servicioIndex++;
}

function actualizarIndices() {
    const rows = document.querySelectorAll('.servicio-row');
    rows.forEach((row, index) => {
        const selects = row.querySelectorAll('select');
        if (selects[0]) selects[0].name = `Asignaciones[${index}].ServicioId`;
        if (selects[1]) selects[1].name = `Asignaciones[${index}].EmpleadoAsignadoId`;
    });
    servicioIndex = rows.length;
}

function validarAsignacion(e) {
    const clienteId = document.querySelector('[name="ClienteId"]');
    if (!clienteId || clienteId.value === '') {
        e.preventDefault();
        alert('Por favor seleccione un cliente');
        return false;
    }
    
    // Verificar al menos un servicio
    const servicios = document.querySelectorAll('[name*="ServicioId"]');
    let hayServicio = false;
    
    servicios.forEach(s => {
        if (s.value !== '0' && s.value !== '') {
            hayServicio = true;
        }
    });
    
    if (!hayServicio) {
        e.preventDefault();
        alert('Por favor seleccione al menos un servicio');
        return false;
    }
    
    return true;
}
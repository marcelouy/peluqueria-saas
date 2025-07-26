// EMPLEADOS VALIDATION - INICIO
console.log('EMPLEADOS VALIDATION - INICIO');

document.addEventListener('DOMContentLoaded', function() {
    console.log('EMPLEADOS VALIDATION - DOM READY');
    
    // Test basico - verificar elementos
    const emailInput = document.getElementById('Email');
    console.log('Email input:', emailInput ? 'ENCONTRADO' : 'NO ENCONTRADO');
    
    if (emailInput) {
        // Patron email mejorado
        const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        
        emailInput.addEventListener('input', function() {
            console.log('Email input event - valor:', this.value);
            
            const email = this.value.trim();
            const emailFeedback = document.getElementById('email-feedback');
            
            // Limpiar clases previas
            this.classList.remove('is-valid', 'is-invalid');
            if (emailFeedback) emailFeedback.textContent = '';
            
            if (email.length > 0) {
                if (emailPattern.test(email)) {
                    this.classList.add('is-valid');
                    if (emailFeedback) {
                        emailFeedback.textContent = 'Email valido';
                        emailFeedback.className = 'text-success small';
                    }
                    console.log('Email valido:', email);
                } else {
                    this.classList.add('is-invalid');
                    if (emailFeedback) {
                        emailFeedback.textContent = 'Email debe tener formato valido (ejemplo@dominio.com)';
                        emailFeedback.className = 'text-danger small';
                    }
                    console.log('Email invalido:', email);
                }
            }
        });
    }
    
    // Validacion fecha nacimiento
    const fechaNacimientoInput = document.getElementById('FechaNacimiento');
    if (fechaNacimientoInput) {
        fechaNacimientoInput.addEventListener('change', function() {
            const fechaSeleccionada = new Date(this.value);
            const fechaHoy = new Date();
            const fechaFeedback = document.getElementById('fecha-feedback');
            
            // Limpiar clases previas
            this.classList.remove('is-valid', 'is-invalid');
            if (fechaFeedback) fechaFeedback.textContent = '';
            
            if (this.value) {
                if (fechaSeleccionada > fechaHoy) {
                    // Fecha futura no permitida
                    this.classList.add('is-invalid');
                    if (fechaFeedback) {
                        fechaFeedback.textContent = 'La fecha de nacimiento no puede ser futura';
                        fechaFeedback.className = 'text-danger small';
                    }
                } else {
                    // Fecha valida
                    this.classList.add('is-valid');
                    if (fechaFeedback) {
                        fechaFeedback.textContent = 'Fecha valida';
                        fechaFeedback.className = 'text-success small';
                    }
                }
            }
        });
    }
    
    // Establecer fecha contratacion default
    const fechaContratacion = document.querySelector('input[name="FechaContratacion"]');
    if (fechaContratacion && !fechaContratacion.value) {
        fechaContratacion.value = new Date().toISOString().split('T')[0];
        console.log('Fecha contratacion default establecida');
    }
    
    console.log('EMPLEADOS VALIDATION - INICIALIZADO');
});

console.log('EMPLEADOS VALIDATION - FIN CARGA');
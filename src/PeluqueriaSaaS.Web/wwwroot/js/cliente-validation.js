// wwwroot/js/cliente-validation.js
// Cliente form validation functionality

document.addEventListener('DOMContentLoaded', function() {
    
    // ✅ EMAIL VALIDATION
    const emailInput = document.querySelector('input[type="email"][name="Email"]');
    if (emailInput) {
        emailInput.addEventListener('input', function() {
            const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
            const isValid = emailPattern.test(this.value);
            
            if (this.value && !isValid) {
                this.setCustomValidity('Email debe incluir dominio completo (ej: .com, .uy, .net)');
            } else {
                this.setCustomValidity('');
            }
        });
        
        // Real-time feedback
        emailInput.addEventListener('blur', function() {
            if (this.value && !this.checkValidity()) {
                this.classList.add('is-invalid');
            } else {
                this.classList.remove('is-invalid');
            }
        });
    }
    
    // ✅ FECHA NACIMIENTO VALIDATION
    const fechaInput = document.getElementById('fechaNacimiento');
    if (fechaInput) {
        // Set max date to today
        const today = new Date();
        const todayString = today.toISOString().split('T')[0];
        fechaInput.setAttribute('max', todayString);
        
        // Validation on change
        fechaInput.addEventListener('change', function() {
            const selectedDate = new Date(this.value);
            const todayDate = new Date();
            
            if (selectedDate > todayDate) {
                this.setCustomValidity('La fecha de nacimiento no puede ser futura');
                this.classList.add('is-invalid');
            } else {
                this.setCustomValidity('');
                this.classList.remove('is-invalid');
            }
        });
    }
    
    // ✅ SUCCESS MESSAGE AUTO-HIDE
    const successAlerts = document.querySelectorAll('.alert-success');
    successAlerts.forEach(alert => {
        setTimeout(() => {
            if (alert) {
                alert.classList.remove('show');
                setTimeout(() => alert.remove(), 300);
            }
        }, 3000);
    });
    
    // ✅ FORM VALIDATION ENHANCEMENT
    const clienteForm = document.querySelector('form');
    if (clienteForm) {
        clienteForm.addEventListener('submit', function(e) {
            const emailInput = this.querySelector('input[name="Email"]');
            const fechaInput = this.querySelector('input[name="FechaNacimiento"]');
            
            let isValid = true;
            
            // Validate email
            if (emailInput && emailInput.value && !emailInput.checkValidity()) {
                emailInput.classList.add('is-invalid');
                isValid = false;
            }
            
            // Validate fecha
            if (fechaInput && fechaInput.value) {
                const selectedDate = new Date(fechaInput.value);
                const today = new Date();
                if (selectedDate > today) {
                    fechaInput.classList.add('is-invalid');
                    isValid = false;
                }
            }
            
            if (!isValid) {
                e.preventDefault();
                alert('Por favor corrija los errores en el formulario antes de continuar.');
            }
        });
    }
});
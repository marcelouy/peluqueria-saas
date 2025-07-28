// SETTINGS-EDIT.JS - Funcionalidad para Settings Edit Form
// Premisa: JavaScript separado + complete file approach + validación completa

$(document).ready(function() {
    console.log('Settings Edit Form loaded');
    initializeSettingsEdit();
});

// ✅ INICIALIZACIÓN SETTINGS EDIT
function initializeSettingsEdit() {
    console.log('Initializing Settings Edit functionality');
    
    // Setup form validation
    setupFormValidation();
    
    // Setup color pickers sync
    setupColorPickerSync();
    
    // Setup character counters
    setupCharacterCounters();
    
    // Setup conditional fields
    setupConditionalFields();
    
    // Setup form submission
    setupFormSubmission();
    
    // Auto-hide alerts
    setTimeout(function() {
        $('.alert-dismissible').fadeOut();
    }, 5000);
}

// ✅ FORM VALIDATION SETUP
function setupFormValidation() {
    console.log('Setting up form validation');
    
    const form = document.getElementById('settingsForm');
    if (!form) return;
    
    // Bootstrap validation
    form.addEventListener('submit', function(event) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }
        form.classList.add('was-validated');
    });
    
    // Custom validations
    setupCustomValidations();
}

// ✅ CUSTOM VALIDATIONS
function setupCustomValidations() {
    // Email validation
    $('#EmailPeluqueria').on('blur', function() {
        const email = $(this).val().trim();
        if (email && !isValidEmail(email)) {
            $(this).addClass('is-invalid');
            $(this).siblings('.invalid-feedback').text('Email inválido');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
    // Phone validation
    $('#TelefonoPeluqueria').on('blur', function() {
        const phone = $(this).val().trim();
        if (phone && !isValidPhone(phone)) {
            $(this).addClass('is-invalid');
            $(this).siblings('.invalid-feedback').text('Formato de teléfono inválido');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
    // Color validation
    $('#ColorPrimarioText, #ColorSecundarioText').on('blur', function() {
        const color = $(this).val().trim();
        if (color && !isValidColor(color)) {
            $(this).addClass('is-invalid');
            $(this).siblings('.invalid-feedback').text('Color inválido (use formato #RRGGBB)');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
    // Retention days validation
    $('#DiasRetencionVentas').on('blur', function() {
        const days = parseInt($(this).val());
        if (days < 30 || days > 3650) {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
}

// ✅ COLOR PICKER SYNC
function setupColorPickerSync() {
    console.log('Setting up color picker synchronization');
    
    // Primary color sync
    $('#ColorPrimario').on('input', function() {
        const color = $(this).val();
        $('#ColorPrimarioText').val(color);
        $('#ColorPrimario').attr('name', 'ColorPrimario');
    });
    
    $('#ColorPrimarioText').on('input', function() {
        const color = $(this).val();
        if (isValidColor(color)) {
            $('#ColorPrimario').val(color);
            $('#ColorPrimario').attr('name', 'ColorPrimario');
        }
    });
    
    // Secondary color sync
    $('#ColorSecundario').on('input', function() {
        const color = $(this).val();
        $('#ColorSecundarioText').val(color);
        $('#ColorSecundario').attr('name', 'ColorSecundario');
    });
    
    $('#ColorSecundarioText').on('input', function() {
        const color = $(this).val();
        if (isValidColor(color)) {
            $('#ColorSecundario').val(color);
            $('#ColorSecundario').attr('name', 'ColorSecundario');
        }
    });
}

// ✅ CHARACTER COUNTERS
function setupCharacterCounters() {
    console.log('Setting up character counters');
    
    // Setup counters for text areas
    setupCharacterCounter('ResumenEncabezado', 500);
    setupCharacterCounter('ResumenPiePagina', 1000);
    setupCharacterCounter('TemplateCustomHTML', 2000);
}

function setupCharacterCounter(fieldId, maxLength) {
    const field = document.getElementById(fieldId);
    if (!field) return;
    
    // Create counter element
    const counter = document.createElement('div');
    counter.className = 'character-count';
    counter.id = fieldId + 'Counter';
    
    // Insert after field
    field.parentNode.insertBefore(counter, field.nextSibling);
    
    // Update counter function
    function updateCounter() {
        const currentLength = field.value.length;
        counter.textContent = `${currentLength}/${maxLength} caracteres`;
        
        if (currentLength > maxLength * 0.9) {
            counter.style.color = '#dc3545';
        } else if (currentLength > maxLength * 0.7) {
            counter.style.color = '#ffc107';
        } else {
            counter.style.color = '#6c757d';
        }
    }
    
    // Initial update
    updateCounter();
    
    // Event listeners
    field.addEventListener('input', updateCounter);
    field.addEventListener('paste', setTimeout(updateCounter, 10));
}

// ✅ CONDITIONAL FIELDS
function setupConditionalFields() {
    console.log('Setting up conditional fields');
    
    // Initial setup
    const resumenEnabled = $('#ResumenServicioHabilitado').is(':checked');
    toggleResumenFields(resumenEnabled);
    
    const customTemplate = $('#UsarTemplateCustom').is(':checked');
    toggleCustomTemplate(customTemplate);
}

// ✅ TOGGLE RESUMEN FIELDS
function toggleResumenFields(enabled) {
    console.log('Toggling resumen fields:', enabled);
    
    const fieldsContainer = document.getElementById('resumenFields');
    const previewBtn = document.getElementById('previewBtn');
    
    if (fieldsContainer) {
        fieldsContainer.style.display = enabled ? 'block' : 'none';
    }
    
    if (previewBtn) {
        previewBtn.style.display = enabled ? 'inline-block' : 'none';
    }
}

// ✅ TOGGLE CUSTOM TEMPLATE
function toggleCustomTemplate(enabled) {
    console.log('Toggling custom template:', enabled);
    
    const templateField = document.getElementById('customTemplateField');
    if (templateField) {
        templateField.style.display = enabled ? 'block' : 'none';
    }
}

// ✅ FORM SUBMISSION
function setupFormSubmission() {
    console.log('Setting up form submission');
    
    $('#settingsForm').on('submit', function(e) {
        console.log('Form submitting...');
        
        // Show loading state
        const submitBtn = $(this).find('button[type="submit"]');
        const originalText = submitBtn.html();
        
        submitBtn.prop('disabled', true);
        submitBtn.html('<i class="fas fa-spinner fa-spin"></i> Guardando...');
        
        // Restore button after 3 seconds if form doesn't submit
        setTimeout(function() {
            submitBtn.prop('disabled', false);
            submitBtn.html(originalText);
        }, 3000);
    });
}

// ✅ PREVIEW RESUMEN
function previewResumen() {
    console.log('Opening resumen preview...');
    
    const resumenEnabled = $('#ResumenServicioHabilitado').is(':checked');
    
    if (!resumenEnabled) {
        showNotification('Debe habilitar el resumen de servicio para ver la vista previa', 'warning');
        return;
    }
    
    // Open preview in new window
    const previewUrl = '/Settings/PreviewResumen';
    window.open(previewUrl, '_blank', 'width=800,height=900,scrollbars=yes');
}

// ✅ RESET TO DEFAULTS
function resetToDefaults() {
    console.log('Resetting to default values...');
    
    if (!confirm('¿Está seguro de que desea restaurar todos los valores por defecto? Esta acción no se puede deshacer.')) {
        return;
    }
    
    // Show loading
    showNotification('Restaurando valores por defecto...', 'info');
    
    // AJAX request to reset
    $.ajax({
        url: '/Settings/Reset',
        type: 'POST',
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            console.log('Reset successful:', response);
            showNotification('Configuración restaurada correctamente', 'success');
            
            // Reload page after delay
            setTimeout(function() {
                location.reload();
            }, 2000);
        },
        error: function(xhr, status, error) {
            console.error('Reset error:', error);
            showNotification('Error al restaurar configuración', 'error');
        }
    });
}

// ✅ VALIDATION UTILITIES
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function isValidPhone(phone) {
    // Uruguay phone patterns
    const phoneRegex = /^(\+598\s?)?(0?9[1-9]|2\d)\s?\d{3}\s?\d{3}$/;
    return phoneRegex.test(phone.replace(/\s/g, ''));
}

function isValidColor(color) {
    const colorRegex = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;
    return colorRegex.test(color);
}

// ✅ NOTIFICATION SYSTEM
function showNotification(message, type) {
    // Remove existing notifications
    $('.alert-notification').remove();
    
    // Determine alert class and icon
    let alertClass, icon;
    switch(type) {
        case 'success':
            alertClass = 'alert-success';
            icon = 'fas fa-check-circle';
            break;
        case 'warning':
            alertClass = 'alert-warning';
            icon = 'fas fa-exclamation-triangle';
            break;
        case 'error':
            alertClass = 'alert-danger';
            icon = 'fas fa-times-circle';
            break;
        case 'info':
        default:
            alertClass = 'alert-info';
            icon = 'fas fa-info-circle';
            break;
    }
    
    // Create notification
    const notification = `
        <div class="alert ${alertClass} alert-dismissible fade show alert-notification" role="alert">
            <i class="${icon}"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Insert at top of card body
    $('.card-body').prepend(notification);
    
    // Auto-hide after 5 seconds
    setTimeout(function() {
        $('.alert-notification').fadeOut();
    }, 5000);
}

// ✅ ERROR HANDLING
function handleAjaxError(xhr, status, error) {
    console.error('AJAX Error:', {
        status: status,
        error: error,
        response: xhr.responseText
    });
    
    let errorMessage = 'Error en la operación';
    
    try {
        const response = JSON.parse(xhr.responseText);
        errorMessage = response.message || errorMessage;
    } catch (e) {
        // Use default message
    }
    
    showNotification(errorMessage, 'error');
}

// ✅ FORM UTILITIES
function serializeFormData(form) {
    const formData = new FormData(form);
    const data = {};
    
    for (let [key, value] of formData.entries()) {
        if (data[key]) {
            if (Array.isArray(data[key])) {
                data[key].push(value);
            } else {
                data[key] = [data[key], value];
            }
        } else {
            data[key] = value;
        }
    }
    
    return data;
}
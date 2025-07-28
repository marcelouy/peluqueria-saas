// SETTINGS.JS - Funcionalidad para Settings Index
// Premisa: JavaScript separado + complete file approach + individual testing

$(document).ready(function() {
    console.log('Settings Index loaded');
    initializeSettingsIndex();
});

// ✅ INICIALIZACIÓN SETTINGS INDEX
function initializeSettingsIndex() {
    console.log('Initializing Settings Index functionality');
    
    // Event listeners
    setupEventListeners();
    
    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert-dismissible').fadeOut();
    }, 5000);
}

// ✅ EVENT LISTENERS
function setupEventListeners() {
    // No additional event listeners needed for index view
    // Toggle functionality is handled inline
}

// ✅ TOGGLE RESUMEN SERVICIO
function toggleResumenServicio(enabled) {
    console.log('Toggling resumen servicio:', enabled);
    
    // Show loading
    const toggleSwitch = document.getElementById('toggleResumen');
    const label = toggleSwitch.nextElementSibling.querySelector('.badge');
    
    // Disable switch during request
    toggleSwitch.disabled = true;
    
    // AJAX request to toggle
    $.ajax({
        url: '/Settings/Toggle',
        type: 'POST',
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            enabled: enabled
        },
        success: function(response) {
            console.log('Toggle successful:', response);
            
            // Update badge
            if (enabled) {
                label.className = 'badge bg-success';
                label.textContent = 'HABILITADO';
            } else {
                label.className = 'badge bg-secondary';
                label.textContent = 'DESHABILITADO';
            }
            
            // Show success message
            showNotification('Configuración actualizada correctamente', 'success');
            
            // Reload page to update UI
            setTimeout(function() {
                location.reload();
            }, 1500);
        },
        error: function(xhr, status, error) {
            console.error('Toggle error:', error);
            
            // Revert switch
            toggleSwitch.checked = !enabled;
            
            // Show error message
            showNotification('Error al actualizar configuración', 'error');
        },
        complete: function() {
            // Re-enable switch
            toggleSwitch.disabled = false;
        }
    });
}

// ✅ SHOW NOTIFICATION
function showNotification(message, type) {
    // Remove existing notifications
    $('.alert-notification').remove();
    
    // Create notification
    const alertClass = type === 'success' ? 'alert-success' : 'alert-danger';
    const icon = type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-triangle';
    
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

// ✅ REFRESH PREVIEW (if available)
function refreshPreview() {
    console.log('Refreshing preview...');
    
    // Check if preview button exists and resumen is enabled
    const previewBtn = document.querySelector('a[href*="PreviewResumen"]');
    if (previewBtn) {
        // Open preview in new tab
        window.open(previewBtn.href, '_blank');
    } else {
        showNotification('Preview no disponible - Resumen deshabilitado', 'error');
    }
}

// ✅ UTILITIES
function formatCurrency(amount, symbol = '$U') {
    return symbol + ' ' + parseFloat(amount).toLocaleString('es-UY', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    });
}

function validateColor(color) {
    const colorRegex = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;
    return colorRegex.test(color);
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
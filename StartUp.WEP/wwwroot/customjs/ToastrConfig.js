// Configure Toastr options
toastr.options = {
    "closeButton": true,
    "debug": false,
    "newestOnTop": false,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "preventDuplicates": true,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "5000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};

// Function to show notification
function showNotification(type, message) {
    switch (type) {
        case 'success':
            toastr.success(message);
            break;
        case 'error':
            toastr.error(message);
            break;
        case 'warning':
            toastr.warning(message);
            break;
        case 'info':
            toastr.info(message);
            break;
        default:
            toastr.info("This is a default message.");
            break;
    }
}

// Show notification if there are messages
@if (TempData["SuccessMessage"] != null) {
    <text>
        $(document).ready(function () {
            showNotification('success', '@TempData["SuccessMessage"]');
        });
    </text>
}

@if (TempData["ErrorMessage"] != null) {
    <text>
        $(document).ready(function () {
            showNotification('error', '@TempData["ErrorMessage"]');
        });
    </text>
}

@if (TempData["WarningMessage"] != null) {
    <text>
        $(document).ready(function () {
            showNotification('warning', '@TempData["WarningMessage"]');
        });
    </text>
}

@if (TempData["InfoMessage"] != null) {
    <text>
        $(document).ready(function () {
            showNotification('info', '@TempData["InfoMessage"]');
        });
    </text>
}

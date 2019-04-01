import toastr from 'toastr';

// Toastr Configuration
toastr.options.closeButton = false;
toastr.options.debug = false;
toastr.options.newestOnTop = false;
toastr.options.progressBar = false;
toastr.options.positionClass = "toast-top-right";
toastr.options.preventDuplicates = false;
toastr.options.onclick = null;
toastr.options.showDuration = "300";
toastr.options.hideDuration = "1000";
toastr.options.timeOut = "5000";
toastr.options.extendedTimeOut = "1000";
toastr.options.showEasing = "swing";
toastr.options.hideEasing = "linear";
toastr.options.showMethod = "fadeIn";
toastr.options.hideMethod = "fadeOut";

export var notification = {
    success: function(message) {
        toastr.success(message);
    },
    error: function(message) {
        toastr.error(message);
    },
    notice: function(message) {
        toastr.info(message);
    }
};

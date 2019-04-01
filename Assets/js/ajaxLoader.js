import $ from 'jquery';

var AJAX_LOADER_ELEMENT = '.ajax_loader';

function createAjaxLoader(loadingElementSelector) {
    var $loaderElement = $(loadingElementSelector);
    var showLoader = function() {
        $loaderElement.fadeIn();
    }
    var hideLoader = function() {
        $loaderElement.fadeOut();
    }

    return function(url, data, onSuccess, onError) {
        showLoader();

        $.ajax({
            url: url,
            dataType: 'json',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: typeof onSuccess !== 'undefined' ?
                function (data) {
                    onSuccess.call(this, data);
                    hideLoader(); 
                } : function () { },
            error: typeof onError !== 'undefined' ?
                function (data) {
                    onError.call(this, data);
                    hideLoader();
                } : function () { }
        });
    }
}

var ajaxLoader = createAjaxLoader(AJAX_LOADER_ELEMENT);

export { ajaxLoader };

import micromodal from 'micromodal';
import $ from 'jquery';
import jsViews from 'jsviews';

// Extend jQuery prototype
jsViews($);

// Initialize the modal handler
micromodal.init();

function prepareModal(title) {
    var isHeaderNeeded = typeof title !== 'undefined';
    if (isHeaderNeeded) {
        $('#wnd-modal header').show();
    } else {
        $('#wnd-modal header').hide();
    }
    $('#wnd-modal-content').text(title || '');
}

export var modal = {
    showTemplate: function(templateName, data, modalTitle) {
        var $content = $('#wnd-modal-content'),
            template = $.templates(templateName);

        prepareModal(modalTitle);
        
        $content.html(template.render(data || { }));

        micromodal.show('#wnd-modal');

        return $content;
    },
    showHtml: function(htmlContent, modalTitle) {
        var $content = $('#wnd-modal-content');
        
        prepareModal(modalTitle);

        $content.html(htmlContent);

        micromodal.show("wnd-modal");

        return $content;
    },
    close: function() {
        micromodal.close();
    }
};

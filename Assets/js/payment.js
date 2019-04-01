Payments = {
    ShowPaymentModal: function(handlerName, additionalData) {
        AjaxLoader.Show();

        $.get("/Payment/ShowPaymentModal/?handler=" + handlerName, function (data) {
            AjaxLoader.Hide();

            var $window = ShowHtmlWindow(data);

            if (additionalData) {
                var $option = $window.find(".payment-option");
                $option.data('additional', additionalData);
            }

            Payments.RegisterPaymentPostHandler($window.find(".payment-form"));
        });
    },

    RegisterButtonHandlers: function(){
        $(".payment-modal-activator").off().click(function () {
            var handler = $(this).data('handler');
            var data = $(this).data('additional');

            Payments.ShowPaymentModal(handler, data);

            return false;
        });
    },

    RegisterPaymentPostHandler: function ($form) {
        $form.submit(function () {
            var $dataField = $form.find("[name=Data]");
            var $checkedField = $form.find("input:checked");

            $dataField.val($checkedField.data('additional'));

            return true;
        });
    }
}
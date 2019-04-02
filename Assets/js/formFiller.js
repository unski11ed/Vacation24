import $ from 'jquery';

export function formFillerInitialize($targetForm){
    var $_form = $targetForm;

    return {
        set: function(data) {
            for (var k in data) {
                if (data.hasOwnProperty(k)) {
                    var $field = $_form.find("[name='" + k + "']");
    
                    if ($field.is(":checkbox") && data[k]) {
                        $field.attr("checked", true);
                        continue;
                    }
    
                    if (data[k] !== null)
                        $_form.find("[name='" + k + "']").val(data[k]).trigger("change");
                }
            }
        },
        get: function() {
            var output = {};

            $($targetForm).find('input,select,textarea').each(function () {
                var $element = $(this);

                var value;
                if ($element.is(':checkbox')) {
                    value = $element.is(':checked');
                }
                else {
                    value = $element.val();
                }

                output[$element.attr('name')] = value;
            });


            return output;
        }
    }
}

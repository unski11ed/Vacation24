import { placeOptions } from './placeOptions';
import { modal } from './modal';
/*
    NOTES:
    - $objectIconsList should point to .objecticonslist
    - tipsy? is missing (there were refreshTipsy calls here)

*/
function PlaceOptionsEditor(placeId, $objectIconsList) {
    var optionsModel = { };

    // Private Functions ==================================
    function getObjectOptions() {
        var options = provideObjectOptions().map(function(optionModelName) {
            return placeOptions.findByModelName(optionModelName);
        });

        return options;
    }

    function spawnManagementWindow(callbackOk) {
        var data = {
            availableOptions: placeOptions.getAll(),
            placeOptions: getObjectOptions()
        }
        var $wndContent = modal.showTemplate('#placeOptionsWindow', data, 'Place Options');
        var $availableOptions = $wndContent.find('.available_options');
        var $placeOptions = $wndContent.find('.place_options');
        var $approveButton = $wndContent.find('.cmd_ok');
        var $cancelButton = $wndContent.find('.cmd_cancel');

        // Available Options Click handler - moves to placeOptions
        $availableOptions.find('.option_icon').on('click', function() {
            var $option = $(this);
            var optionModelName = $option.data('modelName');
            if ($placeOptions.find('[data-model-name=' + optionModelName + ']').length == 0) {
                $option
                    .clone()
                    .appendTo($placeOptions)
                    .on('click', function() {
                        $(this).remove();
                    });
            }
        });

        // Place Options Click handler - removes from the container
        $placeOptions.find('.option_icon').on('click', function() {
            $(this).remove();
        });

        // Approve handler
        $approveButton.on('click', function () {
            var modelOptions = $placeOptions.toArray().map(function(optionElement) {
                var modelName = optionElement.dataset.modelName;
                return placeOptions.findByModelName(modelName);
            });
            
            callbackOk.call(window, modelOptions);

            modal.close();

            return false;
        });
        // Cancel handler
        $cancelButton.click('click', function () {
            modal.close();

            return false;
        });
    }

    function appendIconOptions(options) {
        $objectIconsList.empty();
        options.forEach(function(option) {
            var $newOption = $(
                '<span' +
                '   class="option_icon ' + option.iconClass + '"' +
                '   data-tooltip="' + option.title + '"' +
                '   data-model-name="' + option.modelName + '"' +
                '></span>'
            );
            $objectIconsList
                .append($newOption);
        });
    }

    // Public Interface ===================================
    return {
        showManagementWindow: function() {
            spawnManagementWindow(appendIconOptions);
        },
        assign: function(data) {
            optionsModel = data || { };

            // Turn an object of model props to array of models
            var optionsToAppend = [];
            for (var prop in data) {
                if (data.hasOwnProperty(prop) && data[prop]) {
                    var option = placeOptions.findByModelName(prop);
                    if (option) {
                        optionsToAppend.push(option);
                    }
                }
            }

            appendIconOptions(optionsToAppend);
        },
        fillModelData: function(input) {
            placeOptions.all().forEach(function(option) {
                optionsModel[option.modelName] = false;
                $objectIconsList.find('.option_icon').each(function() {
                    if (option.modelName === this.dataset.modelName) {
                        optionsModel[modelName] = true;
                    }
                });
            });
            
            input.Options = optionsModel;

            return input;
        }
    }
}
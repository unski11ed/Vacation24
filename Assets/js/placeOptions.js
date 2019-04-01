/**
 * Provides Resort options from data populated
 * by the backend in _ScriptsData.cshtml file
 */
export var placeOptions = {
    findByModelName: function(modelName) {
        var output = null;
        window.appData.placeOptionsDefinition.forEach(function (def) {
            if (def.modelName == modelName)
                output = def;
        });
        return output;
    },
    all: function() {
        return window.appData.placeOptionsDefinition;
    }
}

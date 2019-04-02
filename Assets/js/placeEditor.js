import { ajaxRequest } from './ajaxRequest';
import { formFillerInitialize } from './formFiller';
import { notification } from './notification';

export function placeEditorInitialize(placeId, $placeForm) {
    var formFiller = formFillerInitialize($placeForm);
    var place = null;

    return {
        load: function (callbackComplete) {
            ajaxRequest(
                "/Object/Get",
                placeId != undefined ?
                    { id: _placeId } : null,
                function (placeReceived) {
                    place = placeReceived;
                    
                    //Fill form
                    if(placeId !== undefined) {
                        formFiller.set(place);
                    }
    
                    callbackComplete.call(window, place);
                }
            );
        },
        save: function (callbackPreSave, callbackPostSave) {
            callbackPostSave = callbackPostSave || function () { };
    
            var data = formFiller.get();
            if (placeId) {
                data.Id = placeId;
            }
    
            callbackPreSave.call(window, data);
    
            ajaxRequest("/Object/Save", data, function (result) {
                if (result.Status === ResultType.Success) {
                    notification.success("Resort updated successfully");
                    callbackPostSave.call(window, data);
                } else {
                    notification.error("Error: " + result.message);
                }
            });
        },
        isInitialised: function() {
            return place !== null;
        }
    }
}
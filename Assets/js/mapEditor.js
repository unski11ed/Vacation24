import googleMapProvider from 'google-maps';

var POLAND_CENTER_LAT = 52.0692914;
var POLAND_CENTER_LON = 19.4802122;

/*
    NOTES:
    - marker title has been extracted to a provider function,
    previously was provided by an element .objname
    - onMarker update should update elements previously #objectlattitude, #objectlongitude
    - calling update should provide lattitude and longitude similar to above
*/

export function mapEditorInitialize($container, getObjectTitle, onMarkerChanged) {
    googleMapProvider.KEY = window.appData.googleMaps.apiKey;
    googleMapProvider.load(function(google) {
        // Constructor ====================================
        var directionsService = new google.maps.DirectionsService();
        var directionsDisplay = new google.maps.DirectionsRenderer();
    
        var centerPoint = new google.maps.LatLng(POLAND_CENTER_LAT, POLAND_CENTER_LON);
    
        var marker = null;

        var noStreetNames = [{
            featureType: "road",
            elementType: "labels",
            stylers: [{
                visibility: "off"
            }]
        }];
    
        hideLabels = new google.maps.StyledMapType(noStreetNames, {
            name: "hideLabels"
        });
    
        var myOptions = {
            zoom: 6,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            center: centerPoint
        }
    
        var map = new google.maps.Map($container[0], myOptions);
        directionsDisplay.setMap(map);
        map.mapTypes.set('hide_street_names', hideLabels);

        // Private Functions ==============================
        function setMarker(point) {
            if (!marker) {
                marker = new google.maps.Marker({
                    position: point,
                    title: getObjectTitle(),
                    draggable: true,
                    map: map
                });
    
                map.setCenter(marker.getPosition());
    
                google.maps.event.addListener(marker, 'dragend', savePosition);
            } else {
                marker.setPosition(point);
            }
            savePosition();
        }
    
        function savePosition() {
            var lat = marker.getPosition().lat(),
                lng = marker.getPosition().lng();
    
            onMarkerChanged(lat, lng);
        }

        // Public Interface ===============================
        return {
            update: function (lattitude, longitude) {
                var fLattitude = parseFloat(lattitude),
                    fLongitude = parseFloat(longitude);

                //Marker in center if no Lat/Lng set in model
                var markerPoint = isNaN(fLattitude) || isNaN(fLongitude) ?
                    centerPoint : new google.maps.LatLng(fLattitude, fLongitude);
        
                setMarker(markerPoint);
            },
            refresh: function () {
                google.maps.event.trigger(map, 'resize');
        
                map.setCenter(marker.getPosition());
            }
        }
    });
}

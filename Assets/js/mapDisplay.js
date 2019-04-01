import googleMapProvider from 'google-maps';

export function displayMap($container, lattitude, longitude) {
    googleMapProvider.KEY = appData.googleMaps.apiKey;
    googleMapProvider.load(function(google) {
        var directionsDisplay = new google.maps.DirectionsRenderer();

        var centerPoint = new google.maps.LatLng(lattitude, longitude);

        var marker = null;

        //Settings
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

        //Create map
        var map = new google.maps.Map($(".map_go")[0], myOptions);
        directionsDisplay.setMap(map);
        map.mapTypes.set('hide_street_names', hideLabels);

        //Add marker
        marker = new google.maps.Marker({
            position: centerPoint,
            title: $(".objecttitle").val(),
            map: map
        });

        map.setCenter(marker.getPosition());
    });

    return {
        refresh: function() {
            google.maps.event.trigger(map, 'resize');

            map.setCenter(marker.getPosition());
        }
    };
}

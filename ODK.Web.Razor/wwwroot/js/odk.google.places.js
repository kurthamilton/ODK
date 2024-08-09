(function () {
    var $inputs = document.querySelectorAll('[data-location]');
    if ($inputs.length === 0) {
        return;
    }
    
    window.initMap = function () {
        bindInputs();
    };

    function bindInputs() {
        $inputs.forEach($input => {
            const $container = $input.closest('[data-location-container]');
            const $lat = $container.querySelector('[data-location-lat]');
            const $long = $container.querySelector('[data-location-long]');
            const $latlong = $container.querySelector('[data-location-latlong]');
            const $name = $container.querySelector('[data-location-name]');

            const options = {
                fields: ['address_components', 'geometry'],
                strictBounds: false,
            };

            const autocomplete = new google.maps.places.Autocomplete($input, options);            
            google.maps.event.addListener(autocomplete, 'place_changed', () => {
                const place = autocomplete.getPlace();
                const location = place.geometry.location;
                const [lat, long] = [location.lat(), location.lng()];
                $lat.value = lat;
                $long.value = long;                
                $name.value = $input.value;
                $latlong.value = `${lat},${long}`;
                $latlong.dispatchEvent(new Event('change'));
            });

            $input.addEventListener('keydown', e => {
                // do not submit the form if selecting a location with the enter key
                if (e.keyCode === 13) {
                    e.preventDefault();
                }
            }); 
        });
    }
})();
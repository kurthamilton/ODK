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

            const options = {
                fields: ['address_components', 'geometry'],
                strictBounds: false,
            };

            // whether or not a place has been selected using autocomplete
            let placeSelected = false;

            const geocoder = new google.maps.Geocoder();
            const autocomplete = new google.maps.places.Autocomplete($input, options);
            google.maps.event.addListener(autocomplete, 'place_changed', () => {
                placeSelected = true;

                const place = autocomplete.getPlace();                
                const location = place.geometry.location;
                const [lat, long] = [location.lat(), location.lng()];                
                setLocation($container, lat, long);
            });

            $input.addEventListener('keydown', e => {
                // do not submit the form if selecting a location with the enter key
                if (e.keyCode === 13) {
                    e.preventDefault();
                }
            });

            // set lat long when address is entered manually and not selected from autocomplete
            $input.addEventListener('change', () => {
                const value = $input.value;

                // Wait briefly to see if autocomplete triggers
                setTimeout(() => {
                    if (placeSelected) {
                        // place was selected using autocomplete
                        placeSelected = false;
                        return;
                    }

                    const address = value.trim();
                    if (!address) {
                        return;                        
                    }

                    geocoder.geocode({ address }, (results, status) => {
                        if (status === 'OK' && results[0]) {
                            const lat = results[0].geometry.location.lat();
                            const long = results[0].geometry.location.lng();
                            setLocation($container, lat, long);
                        }
                    });
                }, 100);
            });
        });
    }

    function setLocation($container, lat, long) {
        const $lat = $container.querySelector('[data-location-lat]');
        const $long = $container.querySelector('[data-location-long]');
        const $latlong = $container.querySelector('[data-location-latlong]');
        const $name = $container.querySelector('[data-location-name]');

        $lat.value = lat;
        $long.value = long;

        if ($latlong) {
            $latlong.value = `${lat},${long}`;
            $latlong.dispatchEvent(new Event('change'));
        }

        if ($name) {
            $name.value = $input.value;
        }
    }
})();
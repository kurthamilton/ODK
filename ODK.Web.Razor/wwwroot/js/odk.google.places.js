(async function () {
    const $inputs = document.querySelectorAll('[data-location]');
    if ($inputs.length === 0) return;

    const { AutocompleteSuggestion, AutocompleteSessionToken } = await google.maps.importLibrary('places');

    $inputs.forEach($input => bindInput($input));

    function bindInput($input) {        
        // Attempt to hide the native autocomplete - NB some browsers may ignore this
        const $form = $input.closest('form');
        $form.setAttribute('autocomplete', 'off');
        $input.setAttribute('autocomplete', 'off');
        $input.setAttribute('data-bs-toggle', 'dropdown');
        
        const $dropdownContainer = $input.closest('.dropdown');
        const $options = $dropdownContainer.querySelector('[data-location-options]');

        // A session starts when the first suggestions are retrieved and ends when a suggestion is selected
        let sessionToken;

        const dropdown = bootstrap.Dropdown.getOrCreateInstance($input);
        let suggestions = [];
        let selectedIndex = -1;
        
        // Fetch suggestions on focus and input
        ['focus', 'input'].forEach(e => $input.addEventListener(e, loadOptions));        

        // Keep dropdown open if the input still has focus
        $input.addEventListener('hidden.bs.dropdown', () => {
            if (document.activeElement === $input) dropdown.show();
        });
        
        async function loadOptions() {
            const searchTerm = $input.value;

            // session start
            if (searchTerm) {
                if (!sessionToken) sessionToken = new AutocompleteSessionToken();

                const request = {
                    input: searchTerm,
                    sessionToken: sessionToken
                };

                const response = await AutocompleteSuggestion.fetchAutocompleteSuggestions(request);

                suggestions = response.suggestions
                    .map(x => x.placePrediction);
            } else {
                suggestions = [];
            }

            // Rebuild the dropdown
            $options.innerHTML = '';            

            selectedIndex = -1;

            suggestions.forEach(placePrediction => {                
                const $option = document.createElement('li');
                $option.classList.add('dropdown-item');
                $option.innerHTML = placePrediction.text.text;
                $options.appendChild($option);

                $option.addEventListener('click', async () => {
                    await selectOption($input, placePrediction);                    
                });
            });
        }

        $input.addEventListener('keydown', async (e) => {
            const key = e.key;

            if (key === 'ArrowUp') highlightSelectedOption(selectedIndex - 1);
            else if (key === 'ArrowDown') highlightSelectedOption(selectedIndex + 1);
            else if (key === 'Enter') {
                e.preventDefault();
                await selectOption($input, suggestions[selectedIndex]);
                dropdown.hide();
            };
        });

        function highlightSelectedOption(index) {            
            const dropdownItems = $options.querySelectorAll('li');

            const range = [0, dropdownItems.length - 1];

            if (index < range[0] && selectedIndex === range[0]) return;
            else if (index > range[1] && selectedIndex === range[1]) return;

            selectedIndex = index;

            dropdownItems.forEach(($option, i) => {
                $option.classList.remove('active');
                if (i === selectedIndex) $option.classList.add('active');
            });
        }
    }

    async function selectOption($input, placePrediction) {
        if (!placePrediction) return;

        $input.value = placePrediction.text.text;

        const $container = $input.closest('[data-location-container]');
        if (!$container) return;

        const place = placePrediction.toPlace();
        await place.fetchFields({ fields: ['addressComponents', 'location'] });

        // session end
        sessionToken = null;

        const location = parseLocation(place);
        setLocation($container, $input, location);

        $input.blur();
    }

    function parseLocation(place) {
        const location = {};

        if (place.location) {
            location.lat = place.location.lat();
            location.long = place.location.lng();
        }        

        var countryComponent = place.addressComponents.find(c => c.types.includes('country'));
        if (countryComponent) location.countryCode = countryComponent.shortText;

        return location;
    }

    function setLocation($container, $input, location) {
        const $lat = $container.querySelector('[data-location-lat]');
        const $long = $container.querySelector('[data-location-long]');
        const $latlong = $container.querySelector('[data-location-latlong]');
        const $name = $container.querySelector('[data-location-name]');
        const $countryCode = $container.querySelector('[data-location-countryCode]');

        $lat.value = location.lat;
        $long.value = location.long;

        if ($latlong) {
            $latlong.value = `${location.lat},${location.long}`;
            $latlong.dispatchEvent(new Event('change'));
        }

        if ($name) $name.value = $input.value;

        if ($countryCode && location.countryCode) {
            $countryCode.value = location.countryCode;
            $countryCode.dispatchEvent(new Event('change'));
        }
    }
})();
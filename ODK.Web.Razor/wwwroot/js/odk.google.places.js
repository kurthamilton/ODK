(async function () {
    const $inputs = document.querySelectorAll('[data-location]');
    if ($inputs.length === 0) return;

    // The loader is async, so google.maps arrives after this script has run - see _GoogleLocation.
    await window.odkGoogleMapsReady;

    const { AutocompleteSuggestion, AutocompleteSessionToken } = await google.maps.importLibrary('places');

    $inputs.forEach($input => bindInput($input));

    async function bindInput($input) {
        const $container = $input.closest('[data-location-container]');

        // Read once: the anchor is what the server rendered, so it stays put while the member searches.
        const locationBias = parseBias($container);

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

        // Prevent dropdown showing if no options
        $input.addEventListener('show.bs.dropdown', (e) => {
            if ($options.querySelectorAll('[data-location-option]').length === 0) e.preventDefault();
        });

        setDefaults($container);

        async function loadOptions() {
            const searchTerm = $input.value;

            // session start
            if (searchTerm) {
                if (!sessionToken) sessionToken = new AutocompleteSessionToken();

                const request = {
                    input: searchTerm,
                    sessionToken: sessionToken
                };

                if (locationBias) request.locationBias = locationBias;

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
                $option.setAttribute('data-location-option', '');
                $option.innerHTML = placePrediction.text.text;
                $options.appendChild($option);

                $option.addEventListener('click', async () => {
                    await selectOption($container, $input, placePrediction);
                });
            });

            if (suggestions.length) dropdown.show();
            else dropdown.hide();
        }

        $input.addEventListener('keydown', async (e) => {
            const key = e.key;

            if (key === 'ArrowUp') highlightSelectedOption(selectedIndex - 1);
            else if (key === 'ArrowDown') highlightSelectedOption(selectedIndex + 1);
            else if (key === 'Enter') {
                e.preventDefault();
                e.stopImmediatePropagation();
                await selectOption($container, $input, suggestions[selectedIndex]);
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

    async function selectOption($container, $input, placePrediction) {
        if (!placePrediction) return;
        if (!$container) return;

        $input.value = placePrediction.text.text;

        const place = placePrediction.toPlace();
        await place.fetchFields({ fields: ['location', 'displayName'] });

        // session end
        sessionToken = null;

        const location = parseLocation(place);
        $input.blur();

        setLocation($container, $input, location);
    }

    /* The point suggestions are preferred near, written by the server as "lat,long". locationBias rather
       than locationRestriction: the API treats the second as a filter, so a place genuinely somewhere
       else would stop being findable at all. The two cannot both be sent. */
    function parseBias($container) {
        if (!$container) return null;

        const value = $container.getAttribute('data-location-anchor');
        if (!value) return null;

        const parts = value.split(',').map(Number);
        if (parts.length !== 2 || !parts.every(Number.isFinite)) return null;

        return { lat: parts[0], lng: parts[1] };
    }

    function parseLocation(place) {
        const location = {};

        // The id the server re-resolves the place by. Read from the Place rather than kept from the
        // prediction so it travels with the same object the coordinates come from.
        location.externalId = place.id;
        location.name = place.displayName;

        if (place.location) {
            location.lat = place.location.lat();
            location.long = place.location.lng();
        }

        return location;
    }

    async function setDefaults($container) {
        const $defaults = document.querySelector('[data-location-defaults]');
        if (!$defaults) return;

        $defaults.innerHTML = '';

        const lat = $container.querySelector('[data-location-lat]').value;
        const long = $container.querySelector('[data-location-long]').value;

        if (!lat || !long) return;

        const url = $defaults.getAttribute('data-location-defaults')
            .replace('{lat}', lat)
            .replace('{long}', long);
        const response = await fetch(url);
        if (!response.ok) return;
        const html = await response.text();
        $defaults.innerHTML = html;
    }

    function setLocation($container, $input, location) {
        const $lat = $container.querySelector('[data-location-lat]');
        const $long = $container.querySelector('[data-location-long]');
        const $latlong = $container.querySelector('[data-location-latlong]');
        const $name = $container.querySelector('[data-location-name]');
        const $externalId = $container.querySelector('[data-location-external-id]');
        const $mapQuery = $container.querySelector('[data-location-map-query]');

        $lat.value = location.lat;
        $long.value = location.long;

        if ($externalId) $externalId.value = location.externalId ?? '';

        /* The embed takes a place id directly, so the map follows the place, not the typed text - and
           the coordinates where the lookup gave no place id, which is what the server seeds too. */
        if ($mapQuery) {
            $mapQuery.value = location.externalId
                ? `place_id:${location.externalId}`
                : location.lat != null && location.long != null
                    ? `${location.lat},${location.long}`
                    : '';
            $mapQuery.dispatchEvent(new Event('change'));
        }

        if ($latlong) {
            $latlong.value = `${location.lat},${location.long}`;
            $latlong.dispatchEvent(new Event('change'));
        }

        /* Filled from the place, but never over something the member typed: the field arrives holding
           either nothing or the name the previous selection put there. */
        if ($name && (!$name.value || $name.value === $name.dataset.locationNameSource)) {
            $name.value = location.name ?? '';
            $name.dataset.locationNameSource = $name.value;
        }

        setDefaults($container);
    }
})();
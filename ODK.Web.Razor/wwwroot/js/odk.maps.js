(function () {
    const $maps = document.querySelectorAll('[data-map-baseurl]');
    $maps.forEach($map => {
        const sourceSelector = $map.getAttribute('data-map-query');
        if (!sourceSelector) {
            return;
        }

        const $source = document.querySelector(sourceSelector);
        if (!$source) {
            return;
        }

        const baseUrl = $map.getAttribute('data-map-baseurl');

        const $container = $map.closest('[data-map-container]') || $map;

        function updateUrl() {
            const query = $source.value;
            const url = query
                ? baseUrl.replace('{query}', encodeURIComponent(query))
                : '';
            $map.setAttribute('src', url);

            if (url) {
                $container.classList.remove('d-none');
            } else {
                $container.classList.add('d-none');
            }
        }

        $source.addEventListener('change', () => {
            updateUrl();
        });

        updateUrl();
    });
})();
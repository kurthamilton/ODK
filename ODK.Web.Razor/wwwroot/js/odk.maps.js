(function () {
    const $maps = document.querySelectorAll('[data-map-base-url]');
    $maps.forEach($map => {
        const sourceSelector = $map.getAttribute('data-map-query');
        if (!sourceSelector) return;

        const $source = document.querySelector(sourceSelector);
        if (!$source) return;

        // Everything except the place. Appended the same way the server appends it, so a map that
        // follows the picker and one rendered with a place already known come out identical.
        const baseUrl = $map.getAttribute('data-map-base-url');

        const $container = $map.closest('[data-map-container]') || $map;

        function updateUrl() {
            const query = $source.value;
            const url = query
                ? `${baseUrl}&q=${encodeURIComponent(query)}`
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

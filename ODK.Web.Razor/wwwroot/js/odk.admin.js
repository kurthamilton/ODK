(function () {
    bindLiveUrls();

    function bindLiveUrls() {
        const $urls = document.querySelectorAll('[data-live-url-base]');
        $urls.forEach($url => {
            const $container = $url.closest('[data-live-url-container]');
            if (!$container) {
                return;
            }

            const baseUrl = $url.getAttribute('href');

            const $sources = $container.querySelectorAll('[data-live-url-source]');

            const updateUrl = () => {
                let url = baseUrl;
                if (!url.includes('?')) {
                    url += '?';
                }

                $sources.forEach($source => {
                    const key = encodeURIComponent($source.getAttribute('data-live-url-source'));

                    // assume multiple select
                    const $selected = $source.querySelectorAll('option:checked');
                    const values = Array.from($selected)
                        .map(x => x.value);
                    values.forEach(value => {
                        url += `&${key}=${encodeURIComponent(value)}`;
                    });
                }); 

                $url.setAttribute('href', url);
            }

            updateUrl();

            $sources.forEach($source => {                
                $source.addEventListener('change', () => {
                    updateUrl();                    
                });
            });
        });
    }
})();
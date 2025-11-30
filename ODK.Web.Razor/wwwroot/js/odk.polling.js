(function () {
    bindPolling();

    async function bindPolling() {
        const $container = document.querySelector('[data-odk-polling-url]');
        if (!$container) return;

        const interval = parseInt($container.getAttribute('data-odk-polling-interval'));

        const options = {
            $container: $container,
            interval: isNaN(interval) ? 1000 : interval,
            url: $container.getAttribute('data-odk-polling-url')
        };

        await send(options);
    }

    async function send(options) {
        try {
            const response = await fetch(options.url);
            if (response.status >= 200 && response.status < 300) {
                const json = await response.json();

                options.$container.dispatchEvent(new CustomEvent('odk:polling.response', {
                    detail: {
                        json: json
                    }
                }));
            }            
        } finally {
            window.setTimeout(() => send(options), options.interval);
        }         
    }
})();
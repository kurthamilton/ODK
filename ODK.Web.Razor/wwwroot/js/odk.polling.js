(function () {
    bindPolling();

    async function bindPolling() {
        const $container = document.querySelector('[data-odk-polling-url]');
        if (!$container) return;

        const options = {
            $container: $container,
            cancelled: false,
            sendAgain: false,
            sending: false,
            timeoutId: 0,
            url: $container.getAttribute('data-odk-polling-url')
        };

        $container.addEventListener('odk:polling.cancel', () => {
            options.cancelled = true;
            window.clearTimeout(options.timeoutId);
        });

        // Poll now rather than waiting out the interval - raised by whatever has learned there is something
        // to see, so that the interval only has to cover the case where nothing does.
        $container.addEventListener('odk:polling.now', () => schedule(options, 0));

        await send(options);
    }

    /*
     * Read each time rather than once at bind, so a caller can change the interval while polling: a page
     * that has been told it will hear about changes slows down, and one whose connection drops speeds back
     * up.
     */
    function getInterval($container) {
        const interval = parseInt($container.getAttribute('data-odk-polling-interval'));
        return isNaN(interval) ? 1000 : interval;
    }

    // Exactly one timer is ever live. A request in flight when the next one is asked for sets sendAgain, and
    // the request already running re-arms immediately, rather than a second chain starting beside it.
    function schedule(options, delay) {
        if (options.cancelled) return;

        window.clearTimeout(options.timeoutId);
        options.timeoutId = window.setTimeout(() => send(options), delay);
    }

    async function send(options) {
        if (options.cancelled) return;

        if (options.sending) {
            options.sendAgain = true;
            return;
        }

        options.sending = true;

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
            options.sending = false;
            schedule(options, options.sendAgain ? 0 : getInterval(options.$container));
            options.sendAgain = false;
        }
    }
})();

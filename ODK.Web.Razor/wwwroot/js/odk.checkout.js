(function () {
    /* What the fallback poll drops to once the hub connection is up. It then only has to cover a broadcast
       that never arrives - one raised in a process this page is not connected to, or lost while the socket
       was down - so it can be slow. The markup states the without-a-connection interval, which is what a
       browser that never gets a socket keeps. */
    const watchingIntervalMs = 20000;

    bindCheckout();

    function bindCheckout() {
        const $container = document.querySelector('[data-odk-checkout]');
        if (!$container) return;

        const checkout = {
            $container: $container,
            $statuses: $container.querySelectorAll('[data-odk-checkout-status]'),
            connection: null,
            fallbackIntervalMs: $container.getAttribute('data-odk-polling-interval'),
            leaving: false
        };

        hideElements(checkout.$statuses);

        $container.addEventListener('odk:polling.response', e => showStatus(checkout, e.detail.json.status));

        watch(checkout);
    }

    function hideElements($elements) {
        $elements.forEach(x => x.classList.add('d-none'));
    }

    /*
     * Leaving is latched and happens once. A navigation does not stop the document that started it, and
     * three separate things here can report the same terminal status - the poll, the watch, and a reconnect
     * - so a second reload() lands while the first is still in flight and starts it over, which is a loop
     * rather than a double reload.
     *
     * The connection is stopped rather than left to close on its own, because automatic reconnect otherwise
     * recovers it during the navigation, re-reads the same status and reloads on top of it.
     */
    function leave(checkout) {
        if (checkout.leaving) return;

        checkout.leaving = true;
        checkout.$container.dispatchEvent(new Event('odk:polling.cancel'));
        checkout.connection?.stop();
        window.location.reload();
    }

    function pollNow(checkout) {
        checkout.$container.dispatchEvent(new Event('odk:polling.now'));
    }

    function setPollingInterval(checkout, interval) {
        checkout.$container.setAttribute('data-odk-polling-interval', interval);
    }

    function showStatus(checkout, status) {
        if (checkout.leaving) return;

        hideElements(checkout.$statuses);

        checkout.$statuses.forEach(x => {
            if (x.getAttribute('data-odk-checkout-status') !== status) {
                return;
            }

            x.classList.remove('d-none');

            if (x.getAttribute('data-odk-checkout-action') === 'reload') {
                leave(checkout);
            }
        });
    }

    /*
     * Watching is an optimisation over the poll, never a replacement for it: the hub says only that the
     * session moved, and the status the page acts on always comes from the status endpoint. So every failure
     * here is silent - the page carries on polling at the interval the markup states, which is what it did
     * before there was a hub at all.
     */
    async function watch(checkout) {
        const $container = checkout.$container;
        const url = $container.getAttribute('data-odk-checkout-hub-url');
        const method = $container.getAttribute('data-odk-checkout-watch-method');
        const sessionId = $container.getAttribute('data-odk-checkout-session-id');
        if (!url || !method || !sessionId) return;

        const connection = window.odk.signalR.create(url);
        checkout.connection = connection;

        connection.on('checkoutSessionUpdated', () => pollNow(checkout));

        // Nothing is replayed over a reconnect and a new connection is in no group, so a recovered
        // connection re-joins and re-reads rather than trusting that it missed nothing.
        connection.onreconnecting(() => setPollingInterval(checkout, checkout.fallbackIntervalMs));
        connection.onreconnected(() => invokeWatch(checkout, method, sessionId));

        // Fired once automatic reconnect gives up, after which only the poll is left.
        connection.onclose(() => setPollingInterval(checkout, checkout.fallbackIntervalMs));

        try {
            await connection.start();
        } catch {
            setPollingInterval(checkout, checkout.fallbackIntervalMs);
            return;
        }

        await invokeWatch(checkout, method, sessionId);
    }

    // The status the watch returns closes the gap between the page rendering and the socket opening, in
    // which a payment can complete with nothing yet listening.
    async function invokeWatch(checkout, method, sessionId) {
        if (checkout.leaving) return;

        try {
            const status = await checkout.connection.invoke(method, sessionId);
            setPollingInterval(checkout, watchingIntervalMs);
            showStatus(checkout, status);
        } catch {
            pollNow(checkout);
        }
    }
})();

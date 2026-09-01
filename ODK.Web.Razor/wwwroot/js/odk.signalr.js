(function () {
    window.odk = window.odk || {};

    /*
     * A hub connection with the house defaults, so a second consumer does not re-derive them.
     *
     * Built, not started: a caller has to register its handlers before starting, or it misses whatever
     * arrives during the handshake. Automatic reconnect is on because a page can be open across a sleep or
     * a network blip - but nothing may depend on a connection opening at all, so every caller needs a path
     * that works without one.
     */
    window.odk.signalR = {
        create: function (url) {
            return new signalR.HubConnectionBuilder()
                .withUrl(url)
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Warning)
                .build();
        }
    };
})();

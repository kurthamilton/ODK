// Carries a referral from the link a visitor arrived on through to the sign-up form they eventually
// submit, which may be several navigations later.
//
// The referral id arrives as ?utm_source=. It is stripped from the URL with replaceState rather than a
// reload: the visitor sees one page load, not two, and the id doesn't survive in a URL they might copy
// or share. Local storage (not session storage) so it survives closing the tab and coming back.
(function () {
    'use strict';

    var STORAGE_KEY = 'odk.referrer';

    function capture() {
        var params = new URLSearchParams(window.location.search);
        var referrer = params.get('utm_source');
        if (!referrer) return;

        try {
            window.localStorage.setItem(STORAGE_KEY, referrer);
        } catch (e) {
            // Storage unavailable (private mode, blocked cookies). The referral is simply not attributed
            // rather than the page breaking.
        }

        params.delete('utm_source');
        var query = params.toString();
        window.history.replaceState(
            null,
            '',
            window.location.pathname + (query ? '?' + query : '') + window.location.hash);
    }

    function fill() {
        var field = document.querySelector('[data-odk-referrer]');
        if (!field) return;

        try {
            var referrer = window.localStorage.getItem(STORAGE_KEY);
            if (referrer) {
                field.value = referrer;
            }
        } catch (e) {
            // As above - no storage means no attribution, never a broken sign-up.
        }
    }

    // Nothing clears the stored value once it has been used. That leaves a redundant entry behind, which
    // is the cheaper failure: clearing it on submit would leave the referral existing only in the posted
    // form between submit and response, so a back-navigation or a dropped request would lose it for good.
    // A later capture overwrites it, so it can go stale but never wrong for the visit that set it.
    capture();
    fill();
})();

(function () {
    window.odk = window.odk || {};

    /*
     * Shows the feedback a post made by script has to report.
     *
     * The markup is the server's: this asks the feedback page for the same partial a server-rendered page
     * carries and moves the result into the anchor, rather than building a toast here where the two would
     * drift apart. The items are whatever the post's response gave it - nothing here reads them, it hands
     * them straight back - so a new kind of feedback needs no change on this side.
     */
    window.odk.feedback = {
        // Wording of this side's own, for the cases there is no response to report: a request that never
        // reached the server has nothing to say for itself.
        error: async function (message) {
            await window.odk.feedback.show([{ message: message, type: 'Error' }]);
        },

        show: async function (feedback) {
            if (!feedback || !feedback.length) return;

            const $anchor = document.querySelector('[data-toast-anchor]');
            if (!$anchor) {
                console.warn('There is no toast anchor on this page to show feedback in', feedback);
                return;
            }

            const url = $anchor.getAttribute('data-feedback-url');
            const query = new URLSearchParams();
            feedback.forEach((item, i) => {
                query.append(`feedback[${i}].message`, item.message);
                query.append(`feedback[${i}].type`, item.type);
            });

            let html;
            try {
                const response = await fetch(`${url}?${query}`);
                if (!response.ok) {
                    console.warn(`Feedback request to ${url} failed: ${response.status}`);
                    return;
                }

                html = await response.text();
            } catch (e) {
                console.warn(`Feedback request to ${url} could not be read`, e);
                return;
            }

            const $template = document.createElement('template');
            $template.innerHTML = html.trim();

            // Nothing rendered means nothing was worth showing - an item whose type the page did not
            // recognise, say.
            const $group = $template.content.firstElementChild;
            const $toasts = $group ? [...$group.querySelectorAll('[data-toast]')] : [];
            if (!$toasts.length) return;

            /* One group of toasts to an anchor, however many posts report into it. The group is what the
               layout positions - fixed to the bottom of the viewport on a phone - so a second one would sit
               on top of the first rather than below it, and it is also what carries the contrasting theme
               and the spacing between one toast and the next. */
            const $existing = $anchor.querySelector('.toasts');
            if ($existing) {
                $existing.append(...$toasts);
            } else {
                $anchor.appendChild($group);
            }

            // The group states the theme it wants and is given it on load, which an anchor that was empty
            // until now has missed.
            window.odk.themes.setContrastThemes();
            window.odk.utils.bindToasts($toasts);
        }
    };
})();

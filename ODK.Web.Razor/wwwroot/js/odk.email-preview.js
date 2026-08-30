(function () {
    /* Renders the email template form as it currently stands, so an admin can check a change before saving
       it. The server does the rendering - the same code path a real send goes through - and this only moves
       the result into the dialog.

       Do not turn the check below into an early return. A minifier rewrites one as `if (guard) { ...rest... }`,
       which moves every const after it into that block while the hoisted function declarations that read them
       stay outside - valid syntax that throws "not defined" at runtime, with no minifier error. Keeping the
       consts at the top of the function and testing positively means there is no guard to rewrite. The
       optional chaining follows from that: they are resolved before a page without the dialog is ruled out. */
    const LOADING = 'Loading…';

    const $modal = document.querySelector('#email-preview-modal');
    const $triggers = document.querySelectorAll('[data-email-preview-url]');

    const $error = $modal?.querySelector('[data-email-preview-error]');
    const $frame = $modal?.querySelector('[data-email-preview-frame]');
    const $from = $modal?.querySelector('[data-email-preview-from]');
    const $subject = $modal?.querySelector('[data-email-preview-subject]');

    // Nothing to bind on a page without the dialog, and a trigger without one has nowhere to put its result.
    if ($modal && $triggers.length) {
        $triggers.forEach($trigger => {
            $trigger.addEventListener('click', () => preview($trigger));
        });
    }

    async function preview($trigger) {
        const $form = $trigger.closest('form');
        if (!$form) {
            console.warn('A preview trigger outside a form has nothing to render', $trigger);
            return;
        }

        reset();
        bootstrap.Modal.getOrCreateInstance($modal).show();

        try {
            /* The form's own FormData, which is what makes this a preview of what is on screen: the code
               editor writes through to its textarea on every change, so the current content is already in
               there, and a disabled field is left out exactly as it is on save - which is what leaves it
               previewing the wording it inherits rather than an override being turned off. The antiforgery
               token comes along in the same way. */
            const response = await fetch($trigger.dataset.emailPreviewUrl, {
                method: 'POST',
                headers: window.odk.antiforgeryHeaders(),
                body: new FormData($form)
            });

            if (!response.ok) {
                showError(`The preview could not be rendered (${response.status}).`);
                return;
            }

            const preview = await response.json();

            /* EmailPreviewViewModel's property names, camel-cased by the serialiser. Nothing checks the
               pairing, so a rename on either side has to be made on both. */
            $from.textContent = preview.from;
            $subject.textContent = preview.subject;
            $frame.srcdoc = preview.bodyHtml;
        } catch (e) {
            showError('The preview could not be rendered.');
            console.warn('Email preview request failed', e);
        }
    }

    function reset() {
        $error.classList.add('d-none');
        $error.textContent = '';
        $from.textContent = LOADING;
        $subject.textContent = LOADING;
        // Blanked rather than left holding the last preview, which would read as the current one until the
        // new render arrives.
        $frame.srcdoc = '';
    }

    function showError(message) {
        $error.textContent = message;
        $error.classList.remove('d-none');
        $from.textContent = '';
        $subject.textContent = '';
    }
})();

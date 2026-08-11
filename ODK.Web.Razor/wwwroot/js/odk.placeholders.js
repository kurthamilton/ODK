(function () {
    // Returns the caret to the textarea after opening or closing the list. Bootstrap handles the
    // collapse itself; this is only about where focus ends up, so the next thing typed goes into the
    // content - and it stops the toggle holding focus, which would otherwise keep it looking active.
    document.querySelectorAll('[data-placeholder-toggle]').forEach(toggle => {
        const target = document.querySelector(toggle.dataset.placeholderToggle);
        if (!target) return;

        toggle.addEventListener('click', () => target.focus());
    });

    // Inserts a placeholder at the caret of the textarea a [data-placeholder-target] group points at.
    document.querySelectorAll('[data-placeholder-target]').forEach(group => {
        const target = document.querySelector(group.dataset.placeholderTarget);
        if (!target) return;

        group.querySelectorAll('[data-placeholder-insert]').forEach(button => {
            // Clicking would otherwise blur the textarea before the handler runs. The selection
            // survives the blur, but keeping focus means the caret stays visible where it was.
            button.addEventListener('mousedown', event => event.preventDefault());

            button.addEventListener('click', () => {
                const text = button.dataset.placeholderInsert;
                const start = target.selectionStart;
                const end = target.selectionEnd;

                target.value = target.value.slice(0, start) + text + target.value.slice(end);

                // Collapsed after the inserted text, so repeated clicks read left to right rather
                // than each one overwriting the last.
                target.selectionStart = start + text.length;
                target.selectionEnd = target.selectionStart;
                target.focus();

                // Nothing listens for this today, but the value changed without a keystroke, so
                // anything watching the field (validation, dirty tracking) would never hear about it.
                target.dispatchEvent(new Event('input', { bubbles: true }));
            });
        });
    });
})();

(function () {
    // Returns the caret to the content field after opening or closing the list. Bootstrap handles the
    // collapse itself; this is only about where focus ends up, so the next thing typed goes into the
    // content - and it stops the toggle holding focus, which would otherwise keep it looking active.
    // Where a code editor is mounted this still reads correctly: focusing the field hands off to the
    // editor (see odk.code-editor.js), so there is nothing to special-case here.
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

                // Looked up per click rather than once up front: the editor mounts from a separate
                // bundle, so it need not exist when this handler is bound.
                const editor = window.odk.codeEditor?.get(target);
                if (editor) {
                    // Ace owns the caret once it is mounted - the textarea's own selection is
                    // meaningless. insert() replaces the selection and leaves the caret after the
                    // token, matching the plain textarea path below.
                    editor.insert(text);
                    editor.focus();
                } else {
                    const start = target.selectionStart;
                    const end = target.selectionEnd;

                    target.value = target.value.slice(0, start) + text + target.value.slice(end);

                    // Collapsed after the inserted text, so repeated clicks read left to right rather
                    // than each one overwriting the last.
                    target.selectionStart = start + text.length;
                    target.selectionEnd = target.selectionStart;
                    target.focus();
                }

                // The value changed without a keystroke, so nothing would validate it otherwise: a
                // programmatic assignment raises no event, and a field the user never typed in does not
                // raise change on blur either. `change` specifically, because that is what the content
                // field validates on. The library debounces, so a burst of clicks is one check.
                target.dispatchEvent(new Event('change', { bubbles: true }));
            });
        });
    });
})();

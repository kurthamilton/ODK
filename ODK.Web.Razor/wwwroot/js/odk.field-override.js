(function () {
    /* Turns a field on and off from a checkbox: [data-field-override] holds a selector for the field it
       controls. Disabling is what does the work - a disabled field posts nothing, so the server sees the
       value as unset and leaves it inheriting. The field starts in the right state from the server, so this
       only handles the change.

       Where a code editor is mounted over the field, that has to be told too: Ace reads :disabled once when
       it mounts and owns the box the user actually types in. */
    document.querySelectorAll('[data-field-override]').forEach(toggle => {
        const field = document.querySelector(toggle.dataset.fieldOverride);
        if (!field) {
            // Warned rather than ignored: a selector that matches nothing leaves the toggle inert, which
            // looks like a field that refuses to enable rather than like a wiring mistake.
            console.warn(`No field matches '${toggle.dataset.fieldOverride}'; its override toggle will do nothing`);
            return;
        }

        /* Locked means the field may never be typed into, however the toggle is left - a group whose
           subscription does not cover custom emails can turn a customisation off but not write one. So the
           toggle still governs whether the field posts; it just cannot make it editable. */
        const locked = 'fieldOverrideLocked' in toggle.dataset;

        toggle.addEventListener('change', () => {
            const editable = toggle.checked && !locked;

            field.disabled = !editable;

            // Looked up per change rather than once up front: the editor mounts from a separate bundle, so
            // it need not exist when this handler is bound.
            const editor = window.odk.codeEditor?.get(field);
            if (editor) {
                editor.setReadOnly(!editable);
                editor.container.classList.toggle('code-editor-readonly', !editable);
            }

            if (editable) {
                (editor ?? field).focus();
            }
        });
    });
})();

window.odk = window.odk || {};
window.odk.codeEditor = window.odk.codeEditor || {};

(function () {
    /* Declared above the guard below, and they have to stay there. A minifier rewrites an early return
       as `if (guard) { ...rest... }`, which puts anything declared after it inside that block - and
       const is block-scoped, while the function declarations that read it are hoisted to function
       scope. The result is valid syntax that throws "not defined" at runtime, with no minifier error to
       warn you. This file is currently served unminified, so the trap is dormant rather than fixed. */
    const DARK_THEME = 'ace/theme/monokai';
    const LIGHT_THEME = 'ace/theme/textmate';

    /* The editor sizes itself to its content between a floor and a ceiling, both taken from the textarea so
       the markup decides: rows for the floor, data-code-editor-max-lines for the ceiling. These are the
       fallbacks for a field that sets neither. Past the ceiling the editor scrolls internally. */
    const DEFAULT_MIN_LINES = 5;
    const DEFAULT_MAX_LINES = 30;

    const editors = new Map();

    // Ace ships in its own bundle that only the email template pages load, so on any other page there
    // is nothing to mount.
    if (typeof ace === 'undefined') return;

    document.querySelectorAll('textarea[data-code-editor]').forEach(mount);

    document.addEventListener('odk:theme-changed', event => {
        editors.forEach(editor => editor.setTheme(aceTheme(event.detail)));
    });

    // Lets another script act on the editor sitting over a textarea instead of the textarea itself - see
    // odk.placeholders.js. Returns null when a field has no editor, so callers keep their plain
    // textarea path for every other form.
    window.odk.codeEditor.get = $textarea => editors.get($textarea) ?? null;

    function aceTheme(theme) {
        return theme === 'dark' ? DARK_THEME : LIGHT_THEME;
    }

    // The resolved theme, not the stored preference: odk.themes.js falls back to the system setting when
    // nothing is saved, and writes the answer here.
    function currentTheme() {
        return document.querySelector('[data-theme-root]')?.getAttribute('data-bs-theme');
    }

    /* "none" lets the editor grow to whatever it holds, for a field whose content is a whole document rather
       than a fragment. A missing attribute falls back to the cap rather than to no cap, so a field that
       forgets it cannot run away down the page. */
    function maxLines($textarea) {
        const value = $textarea.dataset.codeEditorMaxLines;
        return value === 'none' ? Infinity : Number(value) || DEFAULT_MAX_LINES;
    }

    function mount($textarea) {
        const $container = document.createElement('div');
        $container.className = 'code-editor';
        $textarea.insertAdjacentElement('afterend', $container);

        // Moved out of sight rather than hidden - see .editor-source in _editor-source.scss for why
        // display:none would switch this field's validation off. Out of the tab order because the editor
        // is what a keyboard user should reach.
        $textarea.classList.add('editor-source');
        $textarea.tabIndex = -1;

        const editor = ace.edit($container, {
            mode: `ace/mode/${$textarea.dataset.codeEditor}`,
            theme: aceTheme(currentTheme()),
            // Ace measures wrapped lines as it renders them, so a wrapped line counts as the rows it
            // occupies rather than as one.
            minLines: $textarea.rows || DEFAULT_MIN_LINES,
            maxLines: maxLines($textarea),
            /* Highlighting only, no syntax checking - that is what the worker adds, and turning it on
               needs worker-html.js served as its own file (a Web Worker cannot load out of the bundle)
               plus basePath set so Ace can find it. Its well-formedness rules do line up with the
               server's more closely than you would expect, optional end tags included, but it parses as
               a document and may flag a missing doctype on every one of these fragments. Left off for
               now: the server's check is what gates the save either way - see the htmlcontent provider
               in odk.forms.js. */
            useWorker: false,
            showPrintMargin: false,
            tabSize: 2,
            useSoftTabs: true,
            wrap: true
        });

        // -1 leaves the cursor at the start. The default selects the whole document, so the first
        // keystroke would replace the template.
        editor.setValue($textarea.value, -1);

        // :disabled rather than the property: the form wraps its fields in <fieldset disabled> when the
        // group's subscription does not include custom emails, and that does not set disabled on the
        // individual control. Without this the editor would look editable while the save is refused.
        if ($textarea.matches(':disabled') || $textarea.readOnly) {
            editor.setReadOnly(true);
            $container.classList.add('code-editor-readonly');
        }

        // Written through on every change so whatever submits the form posts current content, even if
        // the editor never lost focus. Silent - raising the event here would validate every keystroke.
        editor.session.on('change', () => { $textarea.value = editor.getValue(); });

        // The field validates on change (data-val-event, set by OdkEmailTemplateTextAreaFor) and assigning
        // value above does not raise it, so it is raised here: the editor losing focus is the real field's
        // equivalent.
        editor.on('blur', () => {
            $textarea.value = editor.getValue();
            $textarea.dispatchEvent(new Event('change', { bubbles: true }));
        });

        // Anything that focuses the real field should land in the editor, which is what the user can
        // see - the validation library focuses the first invalid field after a failed submit, and the
        // placeholder toggle hands focus back to the content. Ace focuses its own inner textarea, so
        // this does not come back round.
        $textarea.addEventListener('focus', () => editor.focus());

        editors.set($textarea, editor);
    }
})();

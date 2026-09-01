(function () {
    const editors = document.querySelectorAll('[data-html-editor]');

    const standardPlugins = 'preview searchreplace autolink autosave save visualblocks visualchars link lists code';

    /* The resolved theme, not the stored preference: odk.themes.js only writes odk.theme when someone picks
       a theme explicitly, and falls back to the system setting otherwise - so reading storage leaves a dark
       page carrying a light editor for anyone who never touched the toggle. It writes the answer here. */
    const dark = document.querySelector('[data-theme-root]')?.getAttribute('data-bs-theme') === 'dark';

    /* Read off the page rather than written out here, so the editor keeps matching the app's typography
       without a second copy of it to update. Colours are left to content_css, which owns the theme. */
    const pageStyles = getComputedStyle(document.body);

    editors.forEach(el => {
        tinymce.init({
            target: el,
            setup: editor => wireValidation(editor, el),
            license_key: 'gpl',
            // Customise link plugin
            link_title: false,
            link_target_list: false,
            relative_urls: false,
            // Customise UI
            plugins: standardPlugins,
            toolbar: 'undo redo | blocks | bold italic | numlist bullist | link | table | forecolor backcolor formatpainter removeformat | code fullscreen preview',
            // newlines
            newline_behavior: 'block',
            /* Appearance. The skin styles the toolbar and the frame; the text is edited inside an iframe the
               page's CSS cannot reach, so content_css is the only way to theme it - and setting one without
               the other leaves a dark toolbar wrapped around a white box. The skin ships a content css of
               its own, but it only handles word wrapping, so the dark background comes from this one.

               Spelled out as a path, and it has to be: TinyMCE 8 dropped the named shortcuts, so a bare
               'dark' is taken for a relative URL, 404s, and silently leaves the content unstyled. Built from
               baseURL rather than written out, so it follows the library wherever it is served from.

               Both are fixed at init: TinyMCE has no runtime setter for either, and swapping them means
               destroying and rebuilding the editor, which loses the undo history and the cursor. So a theme
               changed while editing applies on the next page load rather than immediately. */
            skin: dark ? 'oxide-dark' : 'oxide',
            ...(dark ? { content_css: `${tinymce.baseURL}/skins/content/dark/content.min.css` } : {}),
            content_style: `body { font-family: ${pageStyles.fontFamily}; font-size: ${pageStyles.fontSize}; }`,
            // remove formatting on paste
            paste_remove_spans: true,
            paste_remove_styles: true,
            // Safety: ensure styles are never allowed
            valid_styles: {
                '*': ''
            }
        });
    });

    /* The textarea stays the form field - the validation library attaches to it and the htmlcontent provider
       reads its value - so it has to keep a box on the page and hold current content. TinyMCE gives it
       neither on its own: it hides the target with display:none, which the library reads as hidden and skips
       (reporting the field valid, whatever it holds), and it only writes the editor's content back on submit.
       See .editor-source in _editor-source.scss, which odk.code-editor.js shares. */
    function wireValidation(editor, $textarea) {
        editor.on('init', () => {
            $textarea.classList.add('editor-source');
            // Clears TinyMCE's own inline display:none, which would otherwise beat the class.
            $textarea.style.display = '';
            // Out of the tab order because the editor is what a keyboard user should reach.
            $textarea.tabIndex = -1;

            /* The baseline the changed-only check compares against - see the htmlcontent provider in
               odk.forms.js. Written through the editor rather than left as the markup the server rendered,
               because TinyMCE normalises what it is given: against the original, a field nobody touched
               would look edited and be checked. Saving first puts the baseline through the same path a
               later blur takes, so the two are comparable. */
            editor.save();
            $textarea.defaultValue = $textarea.value;
        });

        /* Written back and raised on blur, which is what the field validates on (data-val-event, set by
           OdkHtmlEditorTextAreaFor): the editor losing focus is the real field's equivalent of a change.
           Assigning value does not raise the event on its own. */
        editor.on('blur', () => {
            editor.save();
            $textarea.dispatchEvent(new Event('change', { bubbles: true }));
        });
    }
})();

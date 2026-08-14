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
})();

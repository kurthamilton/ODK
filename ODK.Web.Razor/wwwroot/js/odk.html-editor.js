﻿(function () {
    const editors = document.querySelectorAll('[data-html-editor]');    

    const standardPlugins = 'preview searchreplace autolink autosave save visualblocks visualchars link lists';

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
            // appearance
            skin: localStorage.getItem('odk.theme') === 'dark' ? 'oxide-dark' : 'oxide'
        });
    });    
})();
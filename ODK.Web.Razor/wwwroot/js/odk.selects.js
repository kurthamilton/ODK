(function () {
    const $selects = document.querySelectorAll('select[multiple],select[data-searchable]');
    $selects.forEach($select => {
        const placeholder = $select.getAttribute('data-placeholder');
        const multiple = $select.hasAttribute('multiple');
        const required = $select.hasAttribute('data-val-required');
        let allowDeselect = true;

        if (!multiple && required) {
            // only use placeholders on required fields until the deselect icon can be positioned
            const $placeholder = $select.querySelector('option[value=""]');
            if ($placeholder) {
                $placeholder.setAttribute('data-placeholder', 'true');
            }

            allowDeselect = !!$placeholder;
        }

        new SlimSelect({
            select: $select,
            settings: {
                // TODO: position deselect
                allowDeselect: false
            }
        });
    });
})();
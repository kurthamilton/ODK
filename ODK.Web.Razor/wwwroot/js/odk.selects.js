(function () {
    const $selects = document.querySelectorAll('select[multiple],select[data-searchable],select[data-select]');
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

        const searchable = multiple || $select.hasAttribute('data-searchable');
        const alwaysOpen = $select.hasAttribute('data-always-open');

        slimSelect = new SlimSelect({
            select: $select,
            settings: {
                allowDeselect: allowDeselect,
                closeOnSelect: !alwaysOpen,
                placeholderText: $select.getAttribute('data-placeholder'),
                showSearch: searchable
            }
        });
    });
})();
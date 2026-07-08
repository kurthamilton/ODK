(function () {
    bindSlimSelects();

    function bindSlimSelect($select) {
        const clearable = $select.hasAttribute('data-clearable');
        let placeholder = $select.getAttribute('data-placeholder');
        const multiple = $select.hasAttribute('multiple');

        let allowDeselect = true;

        if (!multiple && clearable) {
            const $placeholder = $select.querySelector('option[value=""]');
            if ($placeholder) {
                $placeholder.setAttribute('data-placeholder', 'true');
            }

            allowDeselect = !!$placeholder;
        }

        const searchable = multiple || $select.hasAttribute('data-searchable');
        const alwaysOpen = $select.hasAttribute('data-always-open') || multiple;
        const addable = $select.hasAttribute('data-addable');

        if (!placeholder) {
            // set a dummy placeholder to set height
            $select.classList.add('placeholder-hidden');
            placeholder = 'Select';
        }

        let slimSelect;
        const bindSelect = () => {
            if (slimSelect) {
                slimSelect.destroy();
            }

            const $fieldset = $select.closest('fieldset');
            const disabled = !!$fieldset ? $fieldset.hasAttribute('disabled') : false;

            const $options = $select.options;
            const originalValues = Array.from($options).map(x => x.innerText);
            slimSelect = new SlimSelect({
                select: $select,
                events: {
                    addable: addable ? (value) => {
                        return value;
                    } : null,
                    afterChange: (newValue) => {
                        if (!addable) {
                            return;
                        }
                        const newValues = newValue.filter(x => !originalValues.includes(x.text));
                        if (newValues.length === 0) {
                            return;
                        }

                        Array.from($options)
                            .sort((a, b) => a.innerText.localeCompare(b.innerText))
                            .forEach(node => $select.appendChild(node));
                        $select.dispatchEvent(new Event('rebuild'));
                    },
                    search: (search, currentData) => {
                        const searchFilter = slimSelect.events.searchFilter;
                        const data = slimSelect.getData();
                        const optionGroups = data.filter(x => 'options' in x);

                        if (optionGroups.length === 0) {
                            // use default search if no option groups
                            const defaultMatches = slimSelect.store.search(search, searchFilter);
                            return defaultMatches;
                        }

                        const matches = [];

                        data.forEach(optionGroup => {
                            // return a clone of the underlying option group with filtered options if it matches
                            const optionGroupClone = JSON.parse(JSON.stringify(optionGroup));
                            optionGroupClone.options = [];

                            optionGroup.options.forEach(option => {
                                const searchOptions = [option];
                                if (optionGroup.label) {
                                    // include the option group in the search if it has a label as a pseudo-option
                                    searchOptions.push({ text: optionGroup.label });
                                }

                                if (searchOptions.some(x => searchFilter(x, search))) {
                                    optionGroupClone.options.push(option);
                                }
                            });

                            if (optionGroupClone.options.length > 0) {
                                // return the option group if there is at least one matching option
                                matches.push(optionGroupClone);
                            }
                        });

                        return matches;
                    }
                },
                settings: {
                    allowDeselect: allowDeselect,
                    closeOnSelect: !alwaysOpen,
                    disabled: disabled,
                    placeholderText: placeholder,
                    showSearch: searchable
                }
            });
        };

        bindSelect();

        $select.addEventListener('odk:change', e => {
            slimSelect.setSelected(e.detail.values);
        });
        $select.addEventListener('rebuild', () => bindSelect());
    }

    function bindSlimSelects() {
        const $selects = document.querySelectorAll('select[multiple],select[data-searchable],select[data-select]');
        $selects.forEach($select => bindSlimSelect($select));
    }
})();
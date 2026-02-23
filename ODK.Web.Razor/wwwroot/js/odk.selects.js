(function () {
    bindExpandedSelects();
    bindSlimSelects();

    function bindExpandedSelect($select) {
        const id = $select.getAttribute('data-expandable-select');
        const $container = document.querySelector(`[data-expanded-select-for="${id}"]`);
        if (!$container) {
            return;
        }

        const $items = $container.querySelectorAll('[data-expanded-select-option-for]');
        const $groups = $container.querySelectorAll('[data-expanded-select-group]');

        const bindSearch = () => {
            const $search = $container.querySelector('[data-expanded-select-search]');
            if (!$search) {
                return;
            }

            $search.addEventListener('input', () => {
                const search = $search.value.toLowerCase();
                $groups.forEach($group => {
                    const group = $group.getAttribute('data-expanded-select-group');
                    const groupMatch = group && group.toLowerCase().includes(search);

                    const $groupItems = $group.querySelectorAll('[data-expanded-select-option-text]');
                    let visible = 0;

                    $groupItems.forEach($item => {
                        const option = $item.getAttribute('data-expanded-select-option-text');
                        if (!search || groupMatch || option.toLowerCase().includes(search)) {
                            $item.classList.remove('d-none');
                            visible++;
                        } else {
                            $item.classList.add('d-none');
                        }
                    });

                    if (visible > 0) {
                        $group.classList.remove('d-none');
                    } else {
                        $group.classList.add('d-none');
                    }
                });
            });
        };

        const bindSelectedOption = ($item, selectedValues) => {
            const value = $item.getAttribute('data-expanded-select-option-for');

            const $active = $item.querySelector('[data-active]');
            const $inactive = $item.querySelector('[data-inactive]');

            if (selectedValues.includes(value)) {
                $active.classList.remove('d-none');
                $inactive.classList.add('d-none');
            } else {
                $active.classList.add('d-none');
                $inactive.classList.remove('d-none');
            }
        };

        const bindSelectedOptions = () => {
            const selectedValues = getSelectedValues();

            $items.forEach($item => {
                bindSelectedOption($item, selectedValues);

                const value = $item.getAttribute('data-expanded-select-option-for');
                const $active = $item.querySelector('[data-active]');
                const $inactive = $item.querySelector('[data-inactive]');
                $active.addEventListener('click', () => setSelected(value, false));
                $inactive.addEventListener('click', () => setSelected(value, true));
            });
        };

        const getSelectedValues = () => {
            // assume multi select
            return Array.from($select.querySelectorAll('option'))
                .filter(x => x.selected)
                .map(x => x.value);
        };

        const setSelected = (value, selected) => {
            const selectedValues = getSelectedValues();
            if (selected) {
                selectedValues.push(value);
            } else {
                const index = selectedValues.findIndex(x => value === x);
                if (index >= 0) {
                    selectedValues.splice(index, 1);
                }
            }

            $select.dispatchEvent(new CustomEvent('odk:change', {
                detail: {
                    values: selectedValues
                }
            }));
        };

        bindSelectedOptions();
        bindSearch();

        $select.addEventListener('change', () => {
            const selectedValues = getSelectedValues();

            $items.forEach($item => {
                const value = $item.getAttribute('data-expanded-select-option-for');
                const selected = selectedValues.includes(value);
                const $active = $item.querySelector('[data-active]');
                const $inactive = $item.querySelector('[data-inactive]');
                if (selected) {
                    $active.classList.remove('d-none');
                    $inactive.classList.add('d-none');
                } else {
                    $active.classList.add('d-none');
                    $inactive.classList.remove('d-none');
                }
            });
        });
    }

    function bindExpandedSelects() {
        const $selects = document.querySelectorAll('[data-expandable-select]');
        $selects.forEach($select => bindExpandedSelect($select));
    }

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
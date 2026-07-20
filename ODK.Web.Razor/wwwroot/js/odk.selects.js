(function () {
    bindExpandedSelects();

    function bindExpandedSelect($select) {
        const id = $select.getAttribute('data-expandable-select');
        const $container = document.querySelector(`[data-expanded-select-for="${id}"]`);
        if (!$container) return;

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
})();
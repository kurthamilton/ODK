(function () {

    /*COMMON*/
    function stripeTable($table) {
        if (!$table.hasAttribute('data-table-striped')) {
            return;
        }

        const $rows = $table
            .querySelector('tbody')
            .querySelectorAll('tr');

        $rows.forEach($row => $row.classList.remove('table-stripe'));

        let striped = true;
        $rows.forEach($row => {
            if ($row.classList.contains('d-none')) {
                return;
            }

            if (striped) {
                $row.classList.add('table-stripe');
            }            

            striped = !striped;
        });
    }

    document.querySelectorAll('[data-table-striped]')
        .forEach($table => stripeTable($table));

    /*FILTERING*/
    const $filters = document.querySelectorAll('[data-table-filter]');
    $filters.forEach($filter => {
        const targetSelector = $filter.getAttribute('data-table-filter');
        const $target = document.querySelector(targetSelector);
        if (!$target) {
            return;
        }        

        const elementType = $filter.tagName;
        const trigger = elementType === 'SELECT' ? 'change' : 'input';
        $filter.addEventListener(trigger, () => {
            filterTable($target);
        }); 

        filterTable($target);
    });    

    function filterTable($table) {
        const $body = $table.querySelector('tbody');
        const $rows = $body.querySelectorAll('tr');

        const filters = [];
        $filters.forEach($filter => {
            const rawValues = $filter.tagName === 'SELECT'
                ? Array.from($filter.options).filter(x => x.selected).map(x => x.value)
                : [$filter.value ?? ''];

            filters.push({
                field: $filter.getAttribute('data-table-filter-field'),
                values: rawValues.map(x => x.toLocaleLowerCase())
            });
        });        

        $rows.forEach($row => {
            let possibleMatches = 0;
            let matches = 0;

            filters.forEach(filter => {
                const field = filter.field;
                const values = filter.values;

                if (!values.find(x => !!x)) {
                    return;
                }

                possibleMatches++;

                const rowValue = $row.getAttribute(`data-filter-${field}`);
                if (!rowValue) {
                    return;
                }

                if (values.find(x => rowValue.toLocaleLowerCase().includes(x.toLocaleLowerCase()))) {
                    matches++;
                }
            });            

            if (possibleMatches == 0 || matches === possibleMatches) {
                $row.classList.remove('d-none');
            } else {
                $row.classList.add('d-none');
            }
        });

        stripeTable($table);
    }

    /*SORTING*/
    const sortDirections = {
        asc: {
            class: 'sort-asc',
            compare: -1
        },
        desc: {
            class: 'sort-desc',
            compare: 1
        }
    };

    const $lists = document.querySelectorAll('[data-sortable]');
    $lists.forEach($list => {
        const $header = $list.querySelector('thead');
        const $body = $list.querySelector('tbody');
        if (!$header || !$body) {
            return;
        }

        const $triggers = $header.querySelectorAll('th');
        
        const $rows = $body.querySelectorAll('tr');
        
        $triggers.forEach(($trigger, i) => {
            if (!$trigger.hasAttribute('data-sortable-sort')) {
                return;
            }

            $trigger.classList.add('sortable');
            const options = $trigger.getAttribute('data-sortable-sort').split(',');
            if (options.indexOf('default') >= 0) {
                sort($trigger, i);
            }

            $trigger.addEventListener('click', () => {
                sort($trigger, i);
            });
        });

        function sort($trigger, i) {
            const direction = getDirection($trigger);

            $triggers.forEach(x => {
                x.classList.remove(sortDirections.asc.class);
                x.classList.remove(sortDirections.desc.class);
            });            

            $trigger.classList.add(sortDirections[direction].class);

            const sorted = [];

            $rows.forEach($row => {
                const $cell = $row.querySelectorAll('td')[i];
                const $value = $cell.querySelector('[data-sort-value]');
                const value = $value ? $value.getAttribute('data-sort-value') : $cell.innerHTML;                
                sorted.push({ $row: $row, value: value.toString().toLocaleLowerCase().trim() });
            });

            const compareValue = sortDirections[direction].compare;
            sorted.sort((a, b) => a.value.localeCompare(b.value) * compareValue * -1);

            $rows.forEach($row => $row.remove());

            sorted.forEach(row => $body.appendChild(row.$row));

            stripeTable($list);
        }

        function getDirection($trigger) {
            const existingDirection = $trigger.classList.contains(sortDirections.asc.class)
                ? 'asc'
                : $trigger.classList.contains(sortDirections.desc.class)
                ? 'desc'
                : '';

            const direction = existingDirection === 'asc'
                ? 'desc'
                : existingDirection === 'desc'
                ? 'asc'
                : '';

            if (direction) {
                return direction;
            }

            const defaultDir = $trigger.getAttribute('data-sortable-dir');
            return defaultDir || 'asc';
        }
    });
})();
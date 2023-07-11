(function () {

    /*FILTERING*/
    const $filters = document.querySelectorAll('[data-table-filter]');
    $filters.forEach($filter => {
        const targetSelector = $filter.getAttribute('data-table-filter');
        const $target = document.querySelector(targetSelector);
        if (!$target) {
            return;
        }        

        $filter.addEventListener('input', () => {
            filterTable($target, $filter.value);
        });        
    });

    function filterTable($table, value) {
        const $body = $table.querySelector('tbody');
        const $rows = $body.querySelectorAll('tr');
        $rows.forEach($row => {
            const $values = [...$row.querySelectorAll('[data-filter-value]')];
            const values = $values.map(x => x.innerHTML.toLocaleLowerCase());                        
            if (values.find(x => x.includes(value.toLocaleLowerCase()))) {
                $row.classList.remove('d-none');
            } else {
                $row.classList.add('d-none');
            }
        });
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

        const $triggers = $header.querySelectorAll('th[data-sortable-sort]');
        
        const $rows = $body.querySelectorAll('tr');
        
        $triggers.forEach(($trigger, i) => {
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
            const direction = $trigger.classList.contains(sortDirections.asc.class)
                ? 'desc'
                : 'asc';

            $triggers.forEach(x => {
                x.classList.remove(sortDirections.asc.class);
                x.classList.remove(sortDirections.desc.class);
            });            

            $trigger.classList.add(sortDirections[direction].class);

            const sorted = [];

            $rows.forEach($row => {
                const $cell = $row.querySelectorAll('td')[i];
                const $value = $cell.querySelector('[data-sort-value]');
                const value = $value ? $value.innerHTML : $cell.innerHTML;
                sorted.push({ $row: $row, value: value.toString().toLocaleLowerCase().trim() });
            });

            const compareValue = sortDirections[direction].compare;
            sorted.sort((a, b) => a.value.localeCompare(b.value) * compareValue * -1);

            $rows.forEach($row => $row.remove());

            sorted.forEach(row => $body.appendChild(row.$row));
        }
    });
})();
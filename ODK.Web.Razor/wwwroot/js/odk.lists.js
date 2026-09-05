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
    /* Each filter is held with the table it named, and a pass over a table reads only the filters that named
       that table. A page can carry more than one filterable table, and a row of one carries no attribute for
       another's filter to match - so an unscoped pass would empty every table but the one being filtered. */
    const filters = [];
    document.querySelectorAll('[data-table-filter]').forEach($filter => {
        const $target = document.querySelector($filter.getAttribute('data-table-filter'));
        if (!$target) {
            return;
        }

        filters.push({
            $filter: $filter,
            $target: $target,
            field: $filter.getAttribute('data-table-filter-field')
        });
    });

    filters.forEach(filter => {
        const trigger = filter.$filter.tagName === 'SELECT' ? 'change' : 'input';
        filter.$filter.addEventListener(trigger, () => {
            filterTable(filter.$target);
        });
    });

    /* One pass per table, after every filter is held: a pass run mid-registration would not yet see the
       filters still to come, and a control can arrive already narrowed - a select with options selected. */
    new Set(filters.map(filter => filter.$target)).forEach($target => filterTable($target));

    function filterTable($table) {
        const $body = $table.querySelector('tbody');
        const $rows = $body.querySelectorAll('tr');

        const tableFilters = filters
            .filter(filter => filter.$target === $table)
            .map(filter => {
                const $filter = filter.$filter;

                /* A checkbox reports its value whether or not it is checked, so it is read through `checked`
                   instead - unchecked it contributes an empty value, which is what drops it from the pass. */
                const rawValues = $filter.tagName === 'SELECT'
                    ? Array.from($filter.options).filter(x => x.selected).map(x => x.value)
                    : $filter.type === 'checkbox'
                    ? [$filter.checked ? $filter.value : '']
                    : [$filter.value ?? ''];

                return {
                    /* A typed box is a search and matches any part of a row's value; a chosen option names
                       one whole value, and matching part of it would let a choice carry every value it is a
                       prefix of - Incomplete would bring IncompleteExpired with it. */
                    exact: $filter.tagName !== 'INPUT' || $filter.type === 'checkbox',
                    field: filter.field,
                    values: rawValues.map(x => x.toLocaleLowerCase())
                };
            });

        $rows.forEach($row => {
            let possibleMatches = 0;
            let matches = 0;

            tableFilters.forEach(filter => {
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

                const comparand = rowValue.toLocaleLowerCase();
                if (values.find(x => filter.exact ? comparand === x : comparand.includes(x))) {
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

        /* Which rows are on screen is not otherwise observable - the filter inputs are read here rather
           than by anything else, and a row's visibility is a class. Bulk email's header checkbox reports on
           the visible rows, so it needs telling. */
        $table.dispatchEvent(new CustomEvent('odk:table-filtered'));
    }

    /* Runs a filter pass over a table. Exposed because a filter control can be changed by script rather
       than by the admin - see the show-selected-only switch in odk.bulk-email.js - and a programmatic
       change raises no event for the listeners above to hear. */
    window.odk = window.odk || {};
    window.odk.lists = window.odk.lists || {};
    window.odk.lists.filter = $table => filterTable($table);

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

        /* getDirection and sort are consts rather than function declarations, and the order they are declared
           in is load-bearing. A minifier may rewrite the early return above into a positive
           `if ($header && $body) { ... }`, which moves every declaration after the guard into that block -
           while a function *declaration* hoists to the enclosing scope instead, leaving it unable to see the
           block-scoped $triggers and $rows it closes over. Declared this way they travel with the values they
           capture whatever the guard is rewritten to; declared before their first caller because a const has
           no hoisting to fall back on. */
        const getDirection = $trigger => {
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
        };

        const sort = ($trigger, i) => {
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
        };

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
    });
})();
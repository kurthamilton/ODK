(function () {
    /* Bulk email on the members admin page. The email panel and the selection column are rendered with every
       load and revealed together, so turning the mode on is a class change rather than a round-trip and the
       filters and sort order already set up survive it. */
    const $toggle = document.querySelector('[data-bulk-email-toggle]');
    const $panel = document.querySelector('[data-bulk-email-panel]');
    const $table = document.querySelector('[data-bulk-email-table]');
    if (!$toggle || !$panel || !$table) {
        return;
    }

    /* Everything below the guard is a const, and each is declared before whatever calls it - the same shape
       odk.lists.js documents. A minifier may rewrite the early return above into a positive `if`, which puts
       these in a block; a const travels with the values it captures wherever that block ends up, while a
       function declaration hoists out of it and loses sight of them. */
    const queryKey = $toggle.getAttribute('data-bulk-email-toggle');
    const $columns = $table.querySelectorAll('[data-bulk-email-column]');
    const $cancel = $panel.querySelector('[data-bulk-email-cancel]');

    const $selectAll = $table.querySelector('[data-bulk-email-select-all]');
    const $selects = $table.querySelectorAll('[data-bulk-email-select]');
    const $rows = $table.querySelectorAll('tbody tr');
    const $count = $panel.querySelector('[data-bulk-email-count]');
    const $send = $panel.querySelector('[data-bulk-email-send]');

    /* The show-selected-only switch is a table filter like the ones above the table, so odk.lists.js hides
       and shows rows for it too. What is left here is the state around it: writing the data-filter-selected
       it reads, clearing the other filters when it comes on, and dropping it when one of them changes. */
    const $onlySelected = $panel.querySelector('[data-bulk-email-only-selected]');
    const $tableFilters = $onlySelected
        ? document.querySelectorAll(`[data-table-filter="${$onlySelected.getAttribute('data-table-filter')}"]`)
        : [];

    const runFilter = () => window.odk.lists.filter($table);

    const setMode = on => {
        $panel.classList.toggle('d-none', !on);
        $columns.forEach($column => $column.classList.toggle('d-none', !on));
        $toggle.classList.toggle('active', on);

        /* Leaving the mode leaves the table showing everything. Turning the switch off is all that takes:
           it cleared the other filters when it came on, and unsets itself if one of them changes, so
           nothing else can be filtering while it is on. */
        if (!on && $onlySelected?.checked) {
            $onlySelected.checked = false;
            runFilter();
        }

        /* Keeps the address in step with what is on screen, so a reload or a shared link opens the page the
           same way. Replaced rather than pushed: the mode is not somewhere the back button should return to. */
        const url = new URL(window.location.href);
        if (on) {
            url.searchParams.set(queryKey, '');
        } else {
            url.searchParams.delete(queryKey);
        }

        history.replaceState(null, '', url.toString());
    };

    const hidden = $select => $select.closest('tr').classList.contains('d-none');

    /* What the filters currently leave on screen. The header checkbox acts on these and reports on these,
       while the count covers every selection - a member filtered out of view is still a recipient, because
       filtering deliberately does not touch a row's checkbox. */
    const visible = () => Array.from($selects).filter($select => !hidden($select));

    const selected = () => Array.from($selects).filter($select => $select.checked);

    // What the show-selected-only switch filters on. A row with no checkbox - a member who cannot be
    // emailed - reads as unselected, which is what keeps it out.
    const syncRows = () => $rows.forEach($row => {
        const $select = $row.querySelector('[data-bulk-email-select]');
        $row.setAttribute('data-filter-selected', $select?.checked ? 'true' : 'false');
    });

    const syncHeader = () => {
        const $visible = visible();
        const checked = $visible.filter($select => $select.checked).length;

        $selectAll.disabled = $visible.length === 0;
        $selectAll.checked = $visible.length > 0 && checked === $visible.length;
        $selectAll.indeterminate = checked > 0 && checked < $visible.length;
    };

    const syncCount = () => {
        const $selected = selected();
        const hiddenCount = $selected.filter(hidden).length;

        let text = $selected.length === 1 ? '1 member selected' : `${$selected.length} members selected`;
        if (hiddenCount > 0) {
            text += `, including ${hiddenCount} the filters are hiding`;
        }

        $count.textContent = $selected.length > 0 ? text : '';

        /* Left enabled in the markup so the form still works unscripted; disabled from here because an empty
           send is refused by the server anyway, and saying so before the round-trip is kinder. */
        $send.disabled = $selected.length === 0;
    };

    // Whether a filter is filtering anything. Only one that is can take the switch over, which is also what
    // keeps the clearing below from reading back as the admin changing a filter: what it leaves behind is
    // empty by definition.
    const filtering = $filter => $filter.tagName === 'SELECT'
        ? Array.from($filter.options).some(x => x.selected)
        : !!$filter.value;

    const clearTableFilters = () => $tableFilters.forEach($filter => {
        if ($filter === $onlySelected) {
            return;
        }

        if ($filter.tagName === 'SELECT') {
            /* Slim Select draws its own control over the select, so clearing the options underneath it
               would leave the visible control still showing them - odk:change is the hook it listens on. */
            $filter.dispatchEvent(new CustomEvent('odk:change', { detail: { values: [] } }));
        } else {
            $filter.value = '';
        }
    });

    const selectionChanged = () => {
        syncRows();

        // Only while the switch is on does a selection change what is on screen; otherwise the header and
        // the count are all there is to bring up to date.
        if ($onlySelected?.checked) {
            runFilter();
        } else {
            syncHeader();
            syncCount();
        }
    };

    $toggle.addEventListener('click', e => {
        e.preventDefault();
        setMode($panel.classList.contains('d-none'));

        /* The panel opening above the toolbar moves the button out from under both the pointer and its own
           tooltip, and neither a focused nor a hovered trigger gives the tooltip a reason to hide - it is
           left behind at the old position until the next click or scroll. Blurring covers the focus the
           click leaves, and hiding covers the hover, which raises no mouseleave when it is the element that
           moves rather than the pointer. */
        $toggle.blur();
        bootstrap.Tooltip.getInstance($toggle)?.hide();
    });

    $cancel?.addEventListener('click', e => {
        e.preventDefault();
        setMode(false);
    });

    // Absent where the group has the securable but not the feature: the panel offers the upgrade instead of
    // the form, so there is nothing to select for.
    if (!$selectAll) {
        return;
    }

    $selectAll.addEventListener('change', () => {
        visible().forEach($select => { $select.checked = $selectAll.checked; });
        selectionChanged();
    });

    $selects.forEach($select => $select.addEventListener('change', selectionChanged));

    // Raised by odk.lists.js at the end of every filter pass, whoever asked for it.
    $table.addEventListener('odk:table-filtered', () => {
        syncHeader();
        syncCount();
    });

    /* On, the switch is the only filter in play. Off, it leaves the table showing everything, because the
       filters it cleared are still clear. odk.lists.js hears the admin's own click; this runs a pass of its
       own afterwards, since the clearing above happens after that pass has already read them. */
    $onlySelected?.addEventListener('change', () => {
        if ($onlySelected.checked) {
            clearTableFilters();
        }

        runFilter();
    });

    // Any other filter the admin touches takes over, so the switch drops out rather than compounding with it.
    $tableFilters.forEach($filter => {
        if ($filter === $onlySelected) {
            return;
        }

        $filter.addEventListener($filter.tagName === 'SELECT' ? 'change' : 'input', () => {
            if (!$onlySelected.checked || !filtering($filter)) {
                return;
            }

            $onlySelected.checked = false;
            runFilter();
        });
    });

    syncRows();
    syncHeader();
    syncCount();
})();

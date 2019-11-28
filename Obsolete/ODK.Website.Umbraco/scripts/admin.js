(function ($) {
    $(function () {
        bindChosen();
        bindEventDropDown();
        bindFormSubmit();
        bindHtmlEditor();
        bindMembersTable();
        bindNav();
        bindTableFilters();
        bindTableSort();
    });

    function bindChosen() {
        $('.js-chosen').chosen({
            placeholder_text_multiple: ' ',
            placeholder_text_single: ' '
        });
    }

    function bindEventDropDown() {
        var subject = $('#event-invite-email-subject');
        var body = $('#event-invite-email-body');
        var details = $('.js-event-details');
        var events = $('.js-events');

        var selectEvent = function () {
            var eventId = events.val();
            subject.val($('.js-event-invite-subject[data-event-id="' + eventId + '"]').val());

            var html = $('.js-event-invite-body[data-event-id="' + eventId + '"]').val();
            updateHtml(body, html);

            details.addClass('d-none');
            details.filter('[data-event-id="' + eventId + '"]').removeClass('d-none');
        };

        events.on('change', function () {
            selectEvent();
        });

        selectEvent();
    }

    function bindFormSubmit() {
        $('.js-submit-on-change').on('change', function () {
            var form = $(this).closest('form');
            $.ajax({
                data: form.serialize(),
                method: 'POST',
                url: form.attr('action'),
                success: function () {
                }
            });
        });
    }

    function bindHtmlEditor() {
        $('.js-html-editor').trumbowyg({
            semantic: false,
            svgPath: '/css/lib/trumbowyg/trumbowyg.icons.svg'
        });
    }

    function bindMembersTable() {
        var memberIdInputs = $('.js-member-ids');
        var table = $('.js-members-table');
        table.on('filtered', function (e) {
            var rows = e.rows;
            var memberIdString = rows.map(function (row) {
                return row.data('member-id');
            }).join();

            memberIdInputs.val(memberIdString);
        });
    }

    function bindNav() {
        var navbar = $('.navbar--admin');
        var navLinks = $('.nav-link,.dropdown-item', navbar);

        // fix bootstrap 4 bug - remove active class after selection changed
        navbar.on('shown.bs.tab', 'a', function (e) {
            var navLink = $(this);

            navLinks.removeClass('active');
            navLink.addClass('active');

            if (navLink.hasClass('dropdown-item')) {
                // add class to dropdown if drop down
                var navItem = navLink.closest('.nav-item');
                $('.nav-link', navItem).addClass('active');
            }
        });

        // add current nav item to url
        navLinks.on('click', function () {
            window.location.hash = $(this).attr('href');
        });

        // set active nav item from url
        var link = $('.nav-link[href="' + window.location.hash + '"],.dropdown-item[href="' + window.location.hash + '"]', navbar);
        if (link.length) {
            link.tab('show');
        }
    }

    function bindTableFilters() {
        var filters = $('[data-toggle="table-filter"]');
        filters.each(function () {
            var filter = $(this);
            var target = filter.data('target');
            var rows = $('tbody tr', target);
            filter.on('change', function () {
                applyFilter(rows, filters, target);
            });

            applyFilter(rows, filters, target);
        });

        function applyFilter(rows, filters, target) {
            filters = filters.filter('[data-target="' + target + '"]');

            rows.removeClass('d-none');

            var visibleRows = [];

            rows.each(function () {
                var row = $(this);
                var show = true;

                filters.each(function () {
                    var tableFilter = $(this);
                    var filterValues = tableFilter.val();
                    if (!Array.isArray(filterValues) && filterValues) {
                        filterValues = [filterValues];
                    }

                    if (filterValues.length) {
                        var rowValues = row.data(tableFilter.data('field')).toString().split(',');
                        if (show) {
                            var intersect = filterValues.filter(function (value) {
                                return rowValues.indexOf(value.toString()) !== -1;
                            });
                            show = !!intersect.length;
                        }
                    }
                });

                if (show === false) {
                    row.addClass('d-none');
                } else {
                    visibleRows.push(row);
                }
            });

            rows.closest('table').trigger({
                rows: visibleRows,
                type: 'filtered'
            });
        }
    }

    function bindTableSort() {
        var tables = $('.js-table-sortable');
        tables.each(function () {
            var table = $(this);
            var headers = {};

            // disable sort on specified columns
            $('thead th', table).each(function (index) {
                var cell = $(this);
                if (cell.data('sort') == false) {
                    headers[index] = {
                        sorter: false
                    };
                }
            });

            table.tablesorter({
                headers: headers,
                // sort on the first column
                sortList: [[0, 0]]
            });
        })
    }

    function updateHtml(el, html) {
        el.trumbowyg('html', html || '');
    }
})(jQuery);
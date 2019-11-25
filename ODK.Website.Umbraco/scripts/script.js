(function() {
    $(function () {
        bindInstagramFeed();
        bindCustomFileInputs();
        bindDropDownOther();
        bindImageSubmit();
        bindModals();
        bindSearch();
        bindTooltips();
    });

    function bindCustomFileInputs() {
        $('.custom-file-input').on('change',function() {
            var files = $(this)[0].files;
            var fileNames = [];
            for (var i = 0; i < files.length; i++) {
                fileNames.push(files[i].name);
            }

            $(this).next('.custom-file-label').html(fileNames.join('; '));
        });
    }

    function bindDropDownOther() {
        $('[data-other-for]').each(function() {
            var other = $(this);
            var parent = $('#' + other.data('other-for'));
            var match = other.data('other-key');

            setOtherVisibility(parent, match, other);
            parent.on('change', function () {
                setOtherVisibility(parent, match, other);
            });
        });
    }

    function bindImageSubmit() {
        $(document).on('click', '[data-submit]', function() {
            var submitter = $(this);
            var form = submitter.closest('form');
            if (form.length === 0) {
                return;
            }

            var input = $('[data-submit-value]', form);
            if (input.length === 1) {
                input.val($(this).data('value'));
            }

            var ajaxContainer = submitter.closest('[data-ajax-container]');

            onAjaxStart(form);

            $.ajax({
                data: form.serialize(),
                method: 'POST',
                url: form.attr('action'),
                complete: function () {
                    onAjaxEnd(form);
                },
                success: function (response) {
                    ajaxContainer.empty();
                    var child = $(response);
                    ajaxContainer.append(child);
                }
            });
        });
    }

    function bindInstagramFeed() {
        $('[data-instagram-url]').each(function() {
            var container = $(this);
            onAjaxStart(container);

            var url = container.data('instagram-url');
            $.ajax({
                url: url,
                complete: function() {
                    onAjaxEnd(container);
                },
                success: function (data) {
                    var items = data.graphql.user.edge_owner_to_timeline_media.edges;
                    $('.media-item', container).each(function (i) {
                        var item = items[i].node;
                        var mediaItem = $(this);
                        $('img', mediaItem).attr('src', item.thumbnail_src);
                        var link = $('a', mediaItem);
                        link.attr('href', link.attr('href').replace('{code}', item.shortcode));
                    });
                }
            });
        });
    }

    function bindModals() {
        $('.modal.show').modal('show');
    }

    function bindSearch() {
        var targets = $('.search-target');
        if (targets.length === 0) {
            return;
        }

        $('.search').on('input', function () {
            var term = $(this).val().toLowerCase();
            targets.each(function () {
                var target = $(this);

                var show = false;
                $('.search-field', target).each(function() {
                    var field = $(this);
                    if (field.html().toLowerCase().indexOf(term) >= 0) {
                        show = true;
                    }
                });

                if (show) {
                    target.removeClass('d-none');
                } else {
                    target.addClass('d-none');
                }
            });
        });
    }

    function bindTooltips() {
        $(document).tooltip({ selector: '[data-toggle="tooltip"]' });
    }

    function disposeTooltips(target) {
        $('[data-toggle="tooltip"]', target).tooltip('dispose');
    }

    function onAjaxStart(target) {
        disposeTooltips(target);
        target.addClass('loading').addClass('overlay');
    }

    function onAjaxEnd(target) {
        target.removeClass('loading').removeClass('overlay');
    }

    function setOtherVisibility(parent, match, other) {
        var parentValue = parent.val();
        if (parentValue == match) {
            other.show();
        } else {
            other.hide();
        }
    }
})();
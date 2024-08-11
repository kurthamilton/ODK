(function () {
    const $selects = document.querySelectorAll('select[multiple],select[data-searchable]');
    $selects.forEach($select => {
        new SlimSelect({
            select: $select
        });
    });
})();
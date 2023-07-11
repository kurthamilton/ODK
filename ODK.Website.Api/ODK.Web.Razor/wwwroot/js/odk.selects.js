(function () {
    const $selects = document.querySelectorAll('select[multiple]');
    $selects.forEach($select => {
        new SlimSelect({
            select: $select
        });
    });
})();
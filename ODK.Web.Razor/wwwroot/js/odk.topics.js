(function () {
    const $lists = document.querySelectorAll('[data-new-topic-list]');
    $lists.forEach($list => {
        const $rows = $list.querySelectorAll('[data-new-topic-row]');
        $rows.forEach($row => {
            const rowIndex = parseInt($row.getAttribute('data-new-topic-row'));
            if (isNaN(rowIndex)) {
                return;
            }

            const $topicGroup = $row.querySelector('[data-new-topic-group]');
            if (!$topicGroup) {
                return;
            }

            $topicGroup.addEventListener('change', () => {
                const $nextRow = $list.querySelector(`[data-new-topic-row="${rowIndex + 1}"]`);
                if (!$nextRow) {
                    return;
                }

                $nextRow.classList.remove('d-none');
            });
        });        
    });

    function getSelectedValues($categoryPicker) {
        return Array.from($categoryPicker.options)
            .filter(x => x.selected)
            .map(x => x.value || x.text);
    }
})();
(async function () {
    const $placeholder = document.querySelector('[data-tasks-load]');
    if (!$placeholder) return;

    const url = $placeholder.getAttribute('data-tasks-load');
    const response = await fetch(url);
    if (!response.ok) return;

    // The fragment is the whole control and is empty when nothing is outstanding, so there is no empty
    // state to hide - and Bootstrap binds the dropdown through a delegated handler, so nothing to re-init.
    $placeholder.innerHTML = await response.text();
})();

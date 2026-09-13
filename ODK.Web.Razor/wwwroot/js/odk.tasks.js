(async function () {
    const $placeholder = document.querySelector('[data-tasks-load]');
    if (!$placeholder) return;

    const url = $placeholder.getAttribute('data-tasks-load');
    const response = await fetch(url);
    if (!response.ok) return;

    // The fragment is the whole control and is empty when nothing is outstanding, so there is no empty
    // state to hide - and Bootstrap binds the drawer through a delegated handler, so its trigger needs no
    // re-init.
    $placeholder.innerHTML = await response.text();

    /* The two binders that do run once at load have to be re-run over what just arrived. Scroll first,
       while the drawer is still inside the placeholder: attaching moves the node, and a moved node keeps
       its listeners. It measures zero while the drawer is hidden, which is the case its own
       shown.bs.offcanvas handler exists to correct. */
    window.odk.utils.bindScroll($placeholder);
    window.odk.utils.bindAttachTo($placeholder);
})();

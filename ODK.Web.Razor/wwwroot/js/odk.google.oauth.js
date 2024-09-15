// https://medium.com/@leonardosalles/a-guide-to-custom-google-sign-in-button-e7b02c2c5e4f
(function () {
    const $button = document.querySelector('[data-google-oauth]');
    if (!$button) {
        return;        
    }    

    const callback = (response) => {
        const $token = $button.querySelector('[data-google-token]');
        $token.value = response.credential;

        const $form = $token.closest('form');
        $form.dispatchEvent(new Event('oauth'));
    };

    const init = () => {        
        const clientId = $button.getAttribute('data-google-clientid');

        const scriptUrl = 'https://accounts.google.com/gsi/client';
        const script = document.createElement('script');
        script.setAttribute('src', scriptUrl);
        script.onload = () => {            
            window.google.accounts.id.initialize({
                client_id: clientId,
                ux_mode: 'popup',
                callback: callback
            });
            
            const $googleButton = createButton();
            $button.addEventListener('click', () => $googleButton.click());
        };

        document.body.appendChild(script);
    };

    const createButton = () => {
        const $fakeButtonWrapper = $button.querySelector('[data-google-button]');
        
        window.google.accounts.id.renderButton($fakeButtonWrapper, {
            type: 'icon'
        });

        const $googleButton = $fakeButtonWrapper.querySelector("div[role=button]");        
        return $googleButton;
    };

    init();

})();
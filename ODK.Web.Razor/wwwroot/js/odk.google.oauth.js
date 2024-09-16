// https://medium.com/@leonardosalles/a-guide-to-custom-google-sign-in-button-e7b02c2c5e4f
(function () {
    const $button = document.querySelector('[data-oauth-button="Google"]');
    if (!$button) {
        return;        
    }    

    const callback = (response) => {        
        const $form = $button.closest('form');

        const $token = $form.querySelector('[data-oauth-token]');
        $token.value = response.credential;                

        const decoded = window.odk.oauth.parseJwt(response.credential);

        const $email = $form.querySelector('[data-email]');
        if ($email) {
            $email.value = decoded.email;
        }    

        const $firstName = $form.querySelector('[data-firstname]');
        const $lastName = $form.querySelector('[data-lastname]');        

        if ($firstName && $lastName) {
            const name = decoded.name;
            const nameParts = name.split(' ');
            const firstName = nameParts[0];
            const lastName = nameParts.slice(1).join(' ');

            $firstName.value = firstName;
            $lastName.value = lastName;
        }

        const $provider = $form.querySelector('[data-oauth-provider]');
        if ($provider) {
            $provider.value = $button.getAttribute('data-oauth-button');
        }

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
document.addEventListener('DOMContentLoaded', async () => {
    const getById = (id) => document.getElementById(id);

    if (await isConditionalMediationSupported()) {
        const webAuthnSection = getById('webauthn-cui-section');
        webAuthnSection.classList.remove('hide');
    }
    else {
        const webAuthnNotSupportedSection = getById('webauthn-cui-notsupported-section');
        webAuthnNotSupportedSection.classList.remove('hide');
        return;
    }

    async function handleAsyncAction(button, form, asyncAction, originalInnerHtml) {
        Array.from(form.elements).forEach(el => el.disabled = true);
        button.textContent = 'Processing...';
        try {
            await asyncAction();
        } finally {
            Array.from(form.elements).forEach(el => el.disabled = false);
            button.innerHTML = originalInnerHtml;
        }
    }

    const signInButton = getById('signInButton');

    if (signInButton) {
        signInButton.addEventListener('click', async function () {
            await handleAsyncAction(
                this,
                getById('signinForm'),
                () => authenticationWithDiscoverableCredential(),
                this.innerHTML
            );
        });
    }

    await authenticationWithConditionalMediation();
});

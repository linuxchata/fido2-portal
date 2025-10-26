document.addEventListener('DOMContentLoaded', () => {
    const getById = (id) => document.getElementById(id);

    if (isWebAuthnSupported()) {
        const webAuthnSection = getById('auth-dc-section');
        webAuthnSection.classList.remove('hide');
    }
    else {
        const webAuthnNotSupportedSection = getById('auth-dc-notsupported-section');
        webAuthnNotSupportedSection.classList.remove('hide');
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

    const signUpButton = getById('signUpButton');

    if (signUpButton) {
        signUpButton.addEventListener('click', async function () {
            const signUpUserNameInput = getById('signUpUserName');
            const signUpDisplayNameInput = getById('signUpDisplayName');
            const userName = signUpUserNameInput?.value;
            const displayName = signUpDisplayNameInput?.value;

            if (!userName) {
                notify.error('Please enter a username', 'Validation');
                return;
            }

            if (!displayName) {
                notify.error('Please enter a display name', 'Validation');
                return;
            }

            await handleAsyncAction(
                this,
                getById('signupForm'),
                () => registrationOfDiscoverableCredential(userName, displayName),
                this.innerHTML
            );
        });
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
});
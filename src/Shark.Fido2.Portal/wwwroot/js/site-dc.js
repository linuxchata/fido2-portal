document.addEventListener('DOMContentLoaded', () => {
    const getById = (id) => document.getElementById(id);

    if (isWebAuthnSupported()) {
        const authSection = getById('auth-dc-section');
        authSection.classList.remove('hide');
    }
    else {
        const authNotSupportedSection = getById('auth-dc-notsupported-section');
        authNotSupportedSection.classList.remove('hide');
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

            if (!userName || !displayName) {
                notify.error('Please fill in all required fields', 'Validation Error');
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

            toggleCredentialsButtonVisibility();
        });
    }
});
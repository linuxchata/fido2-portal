document.addEventListener('DOMContentLoaded', async () => {
    const getById = (id) => document.getElementById(id);

    if (await isConditionalMediationSupported()) {
        const webAuthnSection = getById('auth-dc-section');
        webAuthnSection.classList.remove('hide');

        await authenticationWithConditionalMediation();
    }
    else {
        const webAuthnNotSupportedSection = getById('auth-cui-notsupported-section');
        webAuthnNotSupportedSection.classList.remove('hide');
    }
});
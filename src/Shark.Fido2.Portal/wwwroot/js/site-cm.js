document.addEventListener('DOMContentLoaded', async () => {
    const getById = (id) => document.getElementById(id);

    if (await isConditionalMediationSupported()) {
        const authSection = getById('auth-dc-section');
        authSection.classList.remove('hide');
    }
    else {
        const authNotSupportedSection = getById('auth-cui-notsupported-section');
        authNotSupportedSection.classList.remove('hide');
    }

    await authenticationWithConditionalMediation();
});
// Authentication with a public key credential using the Web Authentication API

const authenticationTitle = 'Web Authentication';

let currentAbortController = null;

async function authentication(username) {
    const optionsRequest = {
        username: username
    };

    const options = await fetchAssertionOptions(optionsRequest);

    await requestCredential(options);
}

async function authenticationWithDiscoverableCredential() {
    const optionsRequest = {};

    const options = await fetchAssertionOptions(optionsRequest);

    await requestCredential(options);
}

async function authenticationWithConditionalMediation() {
    const optionsRequest = {};

    const options = await fetchAssertionOptions(optionsRequest);

    await requestCredentialForConditionalMediation(options);
}

async function requestCredential(options) {
    cancelConditionalMediation();

    const credentialRequestOptions = {
        publicKey: {
            rpId: options.rpId,
            challenge: toUint8Array(options.challenge),
            allowCredentials: options.allowCredentials.map(credential => ({
                id: toUint8Array(credential.id),
                transports: credential.transports,
                type: credential.type,
            })),
            timeout: options.timeout
        },
    };

    let assertion;
    try {
        assertion = await navigator.credentials.get(credentialRequestOptions);
    }
    catch (error) {
        console.error(error.message);
        notify.error(error.message, authenticationTitle);
        return;
    }

    const credentials = {
        id: assertion.id,
        rawId: toBase64Url(assertion.rawId),
        response: {
            authenticatorData: toBase64Url(assertion.response.authenticatorData),
            clientDataJson: toBase64Url(assertion.response.clientDataJSON),
            signature: toBase64Url(assertion.response.signature),
            userHandle: toBase64Url(assertion.response.userHandle),
        },
        type: assertion.type,
    };

    await fetchAssertionResult(credentials);
}

async function requestCredentialForConditionalMediation(options) {
    cancelConditionalMediation()

    const controller = new AbortController();
    currentAbortController = controller;

    const credentialRequestOptions = {
        publicKey: {
            rpId: options.rpId,
            challenge: toUint8Array(options.challenge),
            allowCredentials: [],
            timeout: options.timeout
        },
        mediation: 'conditional',
        signal: controller.signal
    };

    let assertion;
    try {
        assertion = await navigator.credentials.get(credentialRequestOptions);
    }
    catch (error) {
        currentAbortController = null;

        if (error.name === 'AbortError') {
            console.warn(error.message);
            return;
        }

        console.error(error.message);
        notify.error(error.message, authenticationTitle);
        return;
    }

    currentAbortController = null;

    const credentials = {
        id: assertion.id,
        rawId: toBase64Url(assertion.rawId),
        response: {
            authenticatorData: toBase64Url(assertion.response.authenticatorData),
            clientDataJson: toBase64Url(assertion.response.clientDataJSON),
            signature: toBase64Url(assertion.response.signature),
            userHandle: toBase64Url(assertion.response.userHandle),
        },
        type: assertion.type,
    };

    await fetchAssertionResult(credentials);
}

async function fetchAssertionOptions(optionsRequest) {
    try {
        const response = await fetch('/assertion/options/', {
            method: 'POST',
            headers: {
                'content-type': 'application/json'
            },
            body: JSON.stringify(optionsRequest)
        });

        if (response.ok) {
            return await response.json();
        }
        else {
            notify.error("Error creating authentication options", authenticationTitle);
        }
    } catch (error) {
        console.error(error.message);
        notify.error(error.message, authenticationTitle);
    }
}

async function fetchAssertionResult(credentials) {
    try {
        const response = await fetch('/assertion/result/', {
            method: 'POST',
            headers: {
                'content-type': 'application/json'
            },
            body: JSON.stringify(credentials)
        });

        if (response.ok) {
            window.location.href = `/CredentialDetails?credentialId=${encodeURIComponent(credentials.id)}`;
        }
        else {
            const responseBody = await response.json();
            const errorMessage = `Sign-in failed${responseBody.errorMessage ? `. ${responseBody.errorMessage}` : ''}`;
            console.error(errorMessage);
            notify.error(errorMessage, registrationTitle);
        }
    } catch (error) {
        console.error(error.message);
        notify.error(error.message, authenticationTitle);
    }
}
function cancelConditionalMediation() {
    if (currentAbortController) {
        currentAbortController.abort({ name: 'AbortError', message: 'Conditional mediation was aborted' });
        currentAbortController = null;
    }
}

window.authentication = authentication;
window.authenticationWithDiscoverableCredential = authenticationWithDiscoverableCredential;
window.authenticationWithConditionalMediation = authenticationWithConditionalMediation;

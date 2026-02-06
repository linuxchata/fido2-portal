using Shark.Fido2.Core.Abstractions.Repositories;
using Shark.Fido2.Domain;

namespace Shark.Fido2.Portal.Services;

public sealed class CredentialService : ICredentialService
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly ILogger<CredentialService> _logger;

    public CredentialService(
        ICredentialRepository credentialRepository,
        ILogger<CredentialService> logger)
    {
        _credentialRepository = credentialRepository;
        _logger = logger;
    }

    public async Task<Credential?> Get(byte[] id, CancellationToken cancellationToken)
    {
        var credential = await _credentialRepository.Get(id, cancellationToken);

        if (credential is not null)
        {
            _logger.LogInformation(
                "The credential '{CredentialId}' is found. Created at {CreatedAt}, updated at {UpdatedAt}, last used at {LastUsedAt}",
                credential.CredentialId,
                credential.CreatedAt,
                credential.UpdatedAt,
                credential.LastUsedAt);
        }

        return credential;
    }
}

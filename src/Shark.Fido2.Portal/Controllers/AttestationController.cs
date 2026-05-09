using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shark.Fido2.Core.Abstractions;
using Shark.Fido2.Domain.Options;
using Shark.Fido2.Models.Mappers;
using Shark.Fido2.Models.Requests;
using Shark.Fido2.Models.Responses;
using Shark.Fido2.Portal.Filters;

namespace Shark.Fido2.Portal.Controllers;

/// <summary>
/// Attestation (registration).
/// </summary>
[ApiController]
[Route("[controller]")]
[TypeFilter(typeof(RestApiExceptionFilter))]
public sealed class AttestationController(IAttestation attestation, ILogger<AttestationController> logger) : ControllerBase
{
    private const string SessionName = "WebAuthn.CreateOptions";

    /// <summary>
    /// Gets credential create options.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    /// <response code="200">The credential creation options.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost("options")]
    [ProducesResponseType(typeof(ServerPublicKeyCredentialCreationOptionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OptionsAsync(
        [FromBody] ServerPublicKeyCredentialCreationOptionsRequest request,
        CancellationToken cancellationToken)
    {
        var createOptions = await attestation.BeginRegistration(request.Map(), cancellationToken);

        HttpContext.Session.SetString(SessionName, JsonSerializer.Serialize(createOptions));

        return Ok(createOptions.Map());
    }

    /// <summary>
    /// Creates credential.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    /// <response code="200">The registration result.</response>
    /// <response code="400">If the session is missing or registration fails.</response>
    [HttpPost("result")]
    [ProducesResponseType(typeof(ServerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServerResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResultAsync(
        [FromBody] ServerPublicKeyCredentialAttestation request,
        CancellationToken cancellationToken)
    {
        var createOptionsString = HttpContext.Session.GetString(SessionName);
        if (string.IsNullOrWhiteSpace(createOptionsString))
        {
            return BadRequest(ServerResponse.CreateFailed());
        }

        var createOptions = JsonSerializer.Deserialize<PublicKeyCredentialCreationOptions>(createOptionsString);

        var response = await attestation.CompleteRegistration(request.Map(), createOptions!, cancellationToken);

        HttpContext.Session.Remove(SessionName);

        if (response.IsValid)
        {
            return Ok(ServerResponse.Create());
        }
        else
        {
            logger.LogError("{Message}", response.Message);
            return BadRequest(ServerResponse.CreateFailed(response.Message));
        }
    }
}

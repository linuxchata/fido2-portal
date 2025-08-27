﻿using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shark.Fido2.Core.Abstractions;
using Shark.Fido2.Domain.Options;
using Shark.Fido2.Models.Mappers;
using Shark.Fido2.Models.Requests;
using Shark.Fido2.Models.Responses;
using Shark.Fido2.Portal.Filters;

namespace Shark.Fido2.Portal.Controllers;

/// <summary>
/// Assertion (authentication).
/// </summary>
[Route("[controller]")]
[ApiController]
[TypeFilter(typeof(RestApiExceptionFilter))]
public class AssertionController(IAssertion assertion, ILogger<AssertionController> logger) : ControllerBase
{
    private const string SessionName = "WebAuthn.RequestOptions";

    private readonly IAssertion _assertion = assertion;

    /// <summary>
    /// Gets credential request options.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    [HttpPost("options")]
    public async Task<IActionResult> Options(
        ServerPublicKeyCredentialGetOptionsRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest(ServerResponse.CreateFailed());
        }

        var requestOptions = await _assertion.BeginAuthentication(request.Map(), cancellationToken);

        var response = requestOptions.Map();

        HttpContext.Session.SetString(SessionName, JsonSerializer.Serialize(requestOptions));

        return Ok(response);
    }

    /// <summary>
    /// Validates credential.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The HTTP response.</returns>
    [HttpPost("result")]
    public async Task<IActionResult> Result(
        ServerPublicKeyCredentialAssertion request,
        CancellationToken cancellationToken)
    {
        if (request == null || request.Response == null)
        {
            return BadRequest(ServerResponse.CreateFailed());
        }

        var requestOptionsString = HttpContext.Session.GetString(SessionName);

        if (string.IsNullOrWhiteSpace(requestOptionsString))
        {
            return BadRequest(ServerResponse.CreateFailed());
        }

        var requestOptions = JsonSerializer.Deserialize<PublicKeyCredentialRequestOptions>(requestOptionsString!);

        var response = await _assertion.CompleteAuthentication(request.Map(), requestOptions!, cancellationToken);

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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shark.Fido2.Core.Abstractions;
using Shark.Fido2.Domain;
using Shark.Fido2.Domain.Options;
using Shark.Fido2.Models;
using Shark.Fido2.Models.Requests;
using Shark.Fido2.Models.Responses;
using Shark.Fido2.Portal.Controllers;
using Shark.Fido2.Portal.Services;
using Xunit;

namespace Shark.Fido2.Portal.Tests.Controllers;

public class AttestationControllerTests
{
    private readonly Mock<IAttestation> _mockAttestation;
    private readonly Mock<ISessionService> _mockSessionService;
    private readonly Mock<ILogger<AttestationController>> _mockLogger;
    private readonly AttestationController _controller;

    public AttestationControllerTests()
    {
        _mockAttestation = new Mock<IAttestation>();
        _mockSessionService = new Mock<ISessionService>();
        _mockLogger = new Mock<ILogger<AttestationController>>();
        _controller = new AttestationController(_mockAttestation.Object, _mockSessionService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Options_ReturnsBadRequest_WhenRequestIsNull()
    {
        // Act
        var result = await _controller.Options(null, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ServerResponse<PublicKeyCredentialCreationOptionsResponse>>(badRequestResult.Value);
        Assert.Equal("Error", response.Status);
    }

    [Fact]
    public async Task Options_ReturnsOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new ServerPublicKeyCredentialCreationOptionsRequest
        {
            Username = "testuser",
            DisplayName = "Test User",
            Attestation = "none",
            AuthenticatorSelection = new AuthenticatorSelection
            {
                AuthenticatorAttachment = "platform",
                UserVerification = "preferred"
            }
        };

        var creationOptions = new PublicKeyCredentialCreationOptions
        {
            Challenge = new byte[] { 1, 2, 3 },
            RelyingParty = new PublicKeyCredentialRpEntity { Name = "Test RP", Id = "localhost" },
            User = new PublicKeyCredentialUserEntity { Name = "testuser", Id = new byte[] { 4, 5, 6 }, DisplayName = "Test User" },
            PublicKeyCredentialParams = new List<PublicKeyCredentialParameters>(),
            Timeout = 60000,
            Attestation = AttestationConveyancePreference.None,
            AuthenticatorSelection = new AuthenticatorSelectionCriteria(),
            ExcludeCredentials = new PublicKeyCredentialDescriptor[] { },
            Extensions = new AuthenticationExtensionsClientInputs()
        };

        _mockAttestation.Setup(x => x.BeginRegistration(It.IsAny<PublicKeyCredentialCreationOptionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(creationOptions);

        // Act
        var result = await _controller.Options(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        // var response = Assert.IsType<PublicKeyCredentialCreationOptionsResponse>(okResult.Value);
        
        _mockSessionService.Verify(x => x.SetString("WebAuthn.CreateOptions", It.IsAny<string>()), Times.Once);
    }
}

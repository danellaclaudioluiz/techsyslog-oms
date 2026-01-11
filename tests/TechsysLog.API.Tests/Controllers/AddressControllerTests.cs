using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechsysLog.API.Controllers;
using TechsysLog.API.Models;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.API.Tests.Controllers;

public class AddressControllerTests
{
    private readonly Mock<ICepService> _cepServiceMock;
    private readonly AddressController _controller;

    public AddressControllerTests()
    {
        _cepServiceMock = new Mock<ICepService>();
        _controller = new AddressController(_cepServiceMock.Object);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "user@test.com"),
            new(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetByCep_WithValidCep_ShouldReturnAddress()
    {
        // Arrange
        var cep = "01310100";
        var cepVo = Cep.Create(cep).Value;
        var address = Address.Create(
            cepVo,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP",
            null).Value;

        _cepServiceMock
            .Setup(s => s.GetAddressByCepAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(address));

        // Act
        var result = await _controller.GetByCep(cep, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<AddressDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Street.Should().Be("Avenida Paulista");
        response.Data.City.Should().Be("São Paulo");
    }

    [Fact]
    public async Task GetByCep_WithFormattedCep_ShouldCleanAndProcess()
    {
        // Arrange
        var formattedCep = "01310-100";
        var cepVo = Cep.Create("01310100").Value;
        var address = Address.Create(
            cepVo,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP",
            null).Value;

        _cepServiceMock
            .Setup(s => s.GetAddressByCepAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(address));

        // Act
        var result = await _controller.GetByCep(formattedCep, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<AddressDto>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCep_WithInvalidLength_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidCep = "1234567"; // 7 digits

        // Act
        var result = await _controller.GetByCep(invalidCep, CancellationToken.None);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("8 digits");
    }

    [Fact]
    public async Task GetByCep_WithTooLongCep_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidCep = "123456789"; // 9 digits

        // Act
        var result = await _controller.GetByCep(invalidCep, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetByCep_WithNonExistentCep_ShouldReturnNotFound()
    {
        // Arrange
        var cep = "00000000";

        _cepServiceMock
            .Setup(s => s.GetAddressByCepAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Address>("CEP not found."));

        // Act
        var result = await _controller.GetByCep(cep, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByCep_WithLetters_ShouldCleanAndValidate()
    {
        // Arrange
        var cepWithLetters = "01ABC310100"; // Should become 01310100

        var cepVo = Cep.Create("01310100").Value;
        var address = Address.Create(
            cepVo,
            "Avenida Paulista",
            "1000",
            "Bela Vista",
            "São Paulo",
            "SP",
            null).Value;

        _cepServiceMock
            .Setup(s => s.GetAddressByCepAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(address));

        // Act
        var result = await _controller.GetByCep(cepWithLetters, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<AddressDto>>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCep_ShouldReturnCorrectAddressDto()
    {
        // Arrange
        var cep = "01310100";
        var cepVo = Cep.Create(cep).Value;
        var address = Address.Create(
            cepVo,
            "Avenida Paulista",
            "1578",
            "Bela Vista",
            "São Paulo",
            "SP",
            "Apto 101").Value;

        _cepServiceMock
            .Setup(s => s.GetAddressByCepAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(address));

        // Act
        var result = await _controller.GetByCep(cep, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<AddressDto>>().Subject;
        response.Data!.Cep.Should().Be("01310100");
        response.Data.Street.Should().Be("Avenida Paulista");
        response.Data.Number.Should().Be("1578");
        response.Data.Neighborhood.Should().Be("Bela Vista");
        response.Data.City.Should().Be("São Paulo");
        response.Data.State.Should().Be("SP");
        response.Data.Complement.Should().Be("Apto 101");
    }
}
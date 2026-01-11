using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechsysLog.API.Models;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.API.Controllers;

/// <summary>
/// Address lookup endpoints.
/// </summary>
[Authorize]
public class AddressController : BaseController
{
    private readonly ICepService _cepService;

    public AddressController(ICepService cepService)
    {
        _cepService = cepService;
    }

    /// <summary>
    /// Get address by CEP.
    /// </summary>
    [HttpGet("{cep}")]
    [ProducesResponseType(typeof(ApiResponse<CepAddressResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCep(string cep, CancellationToken cancellationToken)
    {
        // Remove non-numeric characters
        var cleanCep = new string(cep.Where(char.IsDigit).ToArray());

        if (cleanCep.Length != 8)
            return BadRequest(ApiResponse.Fail("CEP must have 8 digits."));

        var cepResult = Cep.Create(cleanCep);

        if (cepResult.IsFailure)
            return BadRequest(ApiResponse.Fail(cepResult.Error ?? "Invalid CEP format."));

        var result = await _cepService.GetAddressByCepAsync(cepResult.Value, cancellationToken);

        if (result.IsFailure)
            return NotFoundResponse(result.Error ?? "CEP not found.");

        var addressInfo = result.Value;

        var response = new CepAddressResponse
        {
            Cep = cleanCep,
            Street = addressInfo.Street,
            Neighborhood = addressInfo.Neighborhood,
            City = addressInfo.City,
            State = addressInfo.State
        };

        return Ok(ApiResponse<CepAddressResponse>.Ok(response));
    }
}

/// <summary>
/// CEP address response model.
/// </summary>
public class CepAddressResponse
{
    public string Cep { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string Neighborhood { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
}
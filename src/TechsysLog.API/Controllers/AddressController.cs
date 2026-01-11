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
    [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
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

        var address = result.Value;

        var dto = new AddressDto
        {
            Cep = address.Cep.Value,
            Street = address.Street,
            Number = address.Number,
            Neighborhood = address.Neighborhood,
            City = address.City,
            State = address.State,
            Complement = address.Complement
        };

        return Ok(ApiResponse<AddressDto>.Ok(dto));
    }
}
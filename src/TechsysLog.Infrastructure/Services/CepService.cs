using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Infrastructure.Services;

/// <summary>
/// ViaCEP API integration service.
/// </summary>
public class CepService : ICepService
{
    private readonly HttpClient _httpClient;

    public CepService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://viacep.com.br/");
    }

    public async Task<Result<CepAddressInfo>> GetAddressByCepAsync(Cep cep, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"ws/{cep.Value}/json/", cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Result.Failure<CepAddressInfo>("Failed to fetch address from ViaCEP.");

            var viaCepResponse = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken: cancellationToken);

            if (viaCepResponse is null || viaCepResponse.Erro)
                return Result.Failure<CepAddressInfo>("CEP not found.");

            var addressInfo = new CepAddressInfo(
                viaCepResponse.Logradouro ?? string.Empty,
                viaCepResponse.Bairro ?? string.Empty,
                viaCepResponse.Localidade ?? string.Empty,
                viaCepResponse.Uf ?? string.Empty);

            return Result.Success(addressInfo);
        }
        catch (HttpRequestException)
        {
            return Result.Failure<CepAddressInfo>("Failed to connect to ViaCEP service.");
        }
        catch (TaskCanceledException)
        {
            return Result.Failure<CepAddressInfo>("Request to ViaCEP timed out.");
        }
    }

    private class ViaCepResponse
    {
        [JsonPropertyName("cep")]
        public string? Cep { get; set; }

        [JsonPropertyName("logradouro")]
        public string? Logradouro { get; set; }

        [JsonPropertyName("complemento")]
        public string? Complemento { get; set; }

        [JsonPropertyName("bairro")]
        public string? Bairro { get; set; }

        [JsonPropertyName("localidade")]
        public string? Localidade { get; set; }

        [JsonPropertyName("uf")]
        public string? Uf { get; set; }

        [JsonPropertyName("erro")]
        public bool Erro { get; set; }
    }
}
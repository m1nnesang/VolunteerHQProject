using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using VolunteerHQ.Core.DTOs.NovaPoshtaDTOs;

namespace VolunteerHQ.Infrastructure.Services;

public class NovaPoshtaService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;


    public NovaPoshtaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["NovaPoshta:ApiKey"]!;
    }
    
    
    
    public async Task<List<CityResponseDto>> GetCity()
    {
        var requestBody = new
        {
            _apiKey = "479e5d30c809098cad772e07e4fa438c",
            modelName = "Address",
            calledMethod = "getCities",
            methodProperties = new { }
        };
        
        var response = await _httpClient.PostAsJsonAsync("https://api.novaposhta.ua/v2.0/json/", requestBody);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<NovaPoshtaResponse>();
        
        return result!.Data
            .Select(c => new CityResponseDto(c.Description, c.Ref))
            .ToList();

    }
    
    //classes for deserialization
    private class NovaPoshtaResponse
    {
        public List<NovaPoshtaCity> Data { get; set; } = new();
    }

    private class NovaPoshtaCity
    {
        public string? Description { get; set; }
        public string? Ref { get; set; }
    }
    
    
}
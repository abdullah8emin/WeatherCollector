using WeatherCollector.Models;
using Microsoft.Extensions.Options;
using WeatherCollector.Interfaces;
using System.Text.Json.Nodes;

namespace WeatherCollector.Services;

public class OpenMeteoApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiConfig _config;

    public OpenMeteoApiService (HttpClient httpClient, IOptions<WeatherApiConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<(string, double)> GetTemperatureAsync(double latitude, double longitude)
    {
        string url = $"{_config.OpenMeteoUrl}?latitude={latitude}&longitude={longitude}&current=temperature_2m";
        string urlCity = $"{_config.BigDataCloudUrl}?latitude={latitude}&longitude={longitude}&localityLanguage=en&key={_config.BigDataCloudApiKey}";
        
        Console.WriteLine($"İstek atılan URL (temp): {url}");
        Console.WriteLine($"İstek atılan URL (cityName): {urlCity}");
        try
        {
            var tempTask = _httpClient.GetStringAsync(url);
            var cityTask = _httpClient.GetStringAsync(urlCity);

            await Task.WhenAll(tempTask, cityTask);

            var tempJson = JsonNode.Parse(tempTask.Result);
            var cityJson = JsonNode.Parse(cityTask.Result);

            string name = cityJson["city"].ToString();

            double temperature = tempJson["current"]["temperature_2m"].GetValue<double>();

            return (name, temperature);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Hatası ({latitude}, {longitude}): {ex.Message}");
            return ("Hata Oluştu", 0);
        }
    }
}
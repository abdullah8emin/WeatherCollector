using WeatherCollector.Models;
using Microsoft.Extensions.Options;
using WeatherCollector.Interfaces;
using System.Text.Json.Nodes;

namespace WeatherCollector.Services;

public class WeatherApiService : IApiService 
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiConfig _config;

    public WeatherApiService(HttpClient httpClient, IOptions<WeatherApiConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<(string, double)> GetTemperatureAsync(double latitude, double longitude)
    {
        string url = $"{_config.WeatherApiUrl}?key={_config.WeatherApiKey}&q={latitude},{longitude}";
        
        if (string.IsNullOrEmpty(_config.WeatherApiUrl)) 
        {
            Console.WriteLine("HATA: WeatherApi ayarı okunamadı, boş geliyor!");
        }
        Console.WriteLine($"İstek atılan tam URL: {url}");

        string responseData = await _httpClient.GetStringAsync(url);
            
        var jsonNode = JsonNode.Parse(responseData);
        string name =  (string)jsonNode["location"]["region"];
        double temperature = jsonNode["current"]["temp_c"].GetValue<double>();
            
        return (name, temperature);
    }
}
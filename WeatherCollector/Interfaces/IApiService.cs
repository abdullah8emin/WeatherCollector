namespace WeatherCollector.Interfaces;

public interface IApiService
{
    Task<(string, double)> GetTemperatureAsync(double latitude, double longitude);
}
namespace WeatherCollector.Models;

public class WeatherResults
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Temperature { get; set; }
    
    public string ThreadName { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
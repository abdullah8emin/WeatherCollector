using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WeatherCollector.Models;
using System.Data;

namespace WeatherCollector.Data;

public class WeatherRepository
{
    private readonly string _connectionString;
    
    public WeatherRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public void SaveResults(WeatherResults result)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("dbo.SaveResults", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            
            command.Parameters.AddWithValue("@Name", result.Name);
            command.Parameters.AddWithValue("@Latitude", result.Latitude);
            command.Parameters.AddWithValue("@Longitude", result.Longitude);
            command.Parameters.AddWithValue("@Temperature", result.Temperature);
            command.Parameters.AddWithValue("@ThreadName", result.ThreadName);
            
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
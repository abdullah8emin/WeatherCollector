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

    public Coordinates GetCoordinates(string tableName)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("dbo.GetCoordinates", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@TableName", tableName);
            
            connection.Open();

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Coordinates
                    {
                        Id = reader.GetInt32(0),
                        Latitude = (float)reader.GetDouble(1),
                        Longitude = (float)reader.GetDouble(2)
                    };
                }
            }
        }
        return null;
    }
    
    public void SaveResultsAndDelete(string tableName, int id, WeatherResults result)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("dbo.SaveResultsAndDelete", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            
            command.Parameters.AddWithValue("@TableName", tableName);
            command.Parameters.AddWithValue("@Id", id);
            
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
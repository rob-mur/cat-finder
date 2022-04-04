using System.Data;
using CuteAnimalFinder.Models;
using Microsoft.Data.SqlClient;

namespace CuteAnimalFinder.Services;

public class PredictionCache : IPredictionCache
{
    private readonly ILogger<PredictionCache> _logger;
    private readonly string _connectionString;
    public PredictionCache(ILogger<PredictionCache> logger, IConfiguration config)
    {
        _logger = logger;
        _connectionString = config.GetSection("PredictionCacheCS").Value!;
    }

    public void AddPrediction(string img, Animal prediction)
    {
        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand("AddPrediction", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@url", img);
        command.Parameters.AddWithValue("@prediction", (int) prediction);
        connection.Open();
        try
        {
            command.ExecuteNonQuery();
        }
        catch (SqlException e)
        {
            _logger.LogWarning("Sql exception on add Prediction: {Message}", e.Message);
        }
    }

    Dictionary<string, Animal> IPredictionCache.GetPredictions(string[] images)
    {
        var predictions = new Dictionary<string, Animal>();
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        var command = new SqlCommand("GetPredictions", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@urls", string.Join("|", images));
        using var objReader = command.ExecuteReader();
        if (!objReader.HasRows) return predictions;
        while (objReader.Read())
        {
            var url = objReader.GetString(objReader.GetOrdinal("Url"));
            var prediction = objReader.GetInt32(objReader.GetOrdinal("Prediction"));
            predictions[url] = (Animal) prediction;
        }

        return predictions;
    }
}

public interface IPredictionCache
{
    Dictionary<string, Animal> GetPredictions(string[] images);
    void AddPrediction(string img, Animal prediction);
}
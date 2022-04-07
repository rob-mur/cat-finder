using System.Data;
using CuteAnimalFinder.Models;
using CuteAnimalFinder.Notifications;
using MediatR;
using Microsoft.Data.SqlClient;

namespace CuteAnimalFinder.Services;

public class PredictionCache : IPredictionCache
{
    private readonly ILogger<PredictionCache> _logger;
    private readonly string _connectionString;
    private readonly IMediator _mediator;
    
    public PredictionCache(ILogger<PredictionCache> logger, IConfiguration config, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
        _connectionString = config.GetSection("PredictionCacheCS").Value!;
    }

    public void AddPrediction(string img, Animal prediction)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand("AddPrediction", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@url", img);
            command.Parameters.AddWithValue("@prediction", (int) prediction);
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (SqlException e)
        {
            _logger.LogWarning("Sql exception on AddPrediction: {Message}", e.Message);
        }
    }

    async Task<Dictionary<string, Animal>> IPredictionCache.GetPredictions(string[] images)
    {
        var predictions = new Dictionary<string, Animal>();
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var command = new SqlCommand("GetPredictions", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@urls", string.Join("|", images));
            await using var objReader = await command.ExecuteReaderAsync();
            if (!objReader.HasRows) return predictions;
            while (objReader.Read())
            {
                var url = objReader.GetString(objReader.GetOrdinal("Url"));
                var prediction = objReader.GetInt32(objReader.GetOrdinal("Prediction"));
                predictions[url] = (Animal) prediction;
            }
        }
        catch (SqlException e)
        {
            await _mediator.Publish(new SqlCrashNotification(e.Message));
            _logger.LogWarning("Sql exception on GetPredictions: {Message}", e.Message);
        }
        

        return predictions;
    }
}

public interface IPredictionCache
{
    Task<Dictionary<string, Animal>> GetPredictions(string[] images);
    void AddPrediction(string img, Animal prediction);
}
using System.Data;
using System.Data.SqlClient;
using CuteAnimalFinder.Models;

namespace CuteAnimalFinder.Services;

public class PredictionCache : IPredictionCache
{
    private readonly SqlConnectionStringBuilder _builder;
    private readonly ILogger<PredictionCache> _logger;

    public PredictionCache(ILogger<PredictionCache> logger)
    {
        _logger = logger;
        _builder = new SqlConnectionStringBuilder
        {
            DataSource = "prediction-cache.database.windows.net",
            UserID = "verybasicusername",
            Password = "verybasicpassword!1",
            InitialCatalog = "prediction-cache"
        };
    }

    public void AddPrediction(string img, Animal prediction)
    {
        using var connection = new SqlConnection(_builder.ConnectionString);
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
        using var connection = new SqlConnection(_builder.ConnectionString);
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
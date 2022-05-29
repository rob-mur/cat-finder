using System.Security.Cryptography;
using CuteAnimalFinder.Models;
using Microsoft.Data.SqlClient;

namespace CuteAnimalFinder.Services;

public class DbService: IDbService
{

    private readonly IConfiguration _config;
    private readonly PredictionDbContext _dbContext;
    public DbService(IConfiguration config, PredictionDbContext dbContext)
    {
        _config = config;
        _dbContext = dbContext;
    }

    private async Task AddPredictionBackground(Models.ImagePrediction imagePrediction)
    {
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(imagePrediction.Url);
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var sha1 = SHA1.HashData(imageBytes);
            var existingRecord = _dbContext.Predictions.FirstOrDefault(x => x.Sha1 == sha1);
            if (existingRecord != null)
            {
                existingRecord.Votes[imagePrediction.Prediction] += 1;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                var newRecord = new ImagePrediction(sha1, new Dictionary<Animal, int>()
                {
                    {Animal.Cat, 0},
                    {Animal.Dog, 0},
                    {Animal.Neither, 0}
                }, imagePrediction.Url!);
                newRecord.Votes[imagePrediction.Prediction] += 1;
                await _dbContext.AddAsync(newRecord);
                await _dbContext.SaveChangesAsync();
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
        }
        catch (SqlException e)
        {
            Console.WriteLine(e);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e);
        }
        
    }

    public void AddPrediction(Models.ImagePrediction imagePrediction)
    {
        Task.Run(() => AddPredictionBackground(imagePrediction));
    }
}

public interface IDbService
{
    void AddPrediction(Models.ImagePrediction imagePrediction);
}
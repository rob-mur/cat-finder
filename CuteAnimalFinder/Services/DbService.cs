using System.Security.Cryptography;
using CuteAnimalFinder.Models;

namespace CuteAnimalFinder.Services;

public class DbService: IDbService
{
    private readonly PredictionDbContext _db;
    public DbService(IConfiguration config)
    {
        _db = new PredictionDbContext(config);
    }
    public async Task AddPrediction(ImagePrediction imagePrediction)
    {
        _db.Predictions.Add(imagePrediction);
        using var client = new HttpClient();
        using var response = await client.GetAsync(imagePrediction.Url);
        var imageBytes = await response.Content.ReadAsByteArrayAsync();
        var sha1 = SHA1.HashData(imageBytes);
        var existingRecord = _db.NewPredictions.FirstOrDefault(x => x.Sha1 == sha1);
        if (existingRecord != null)
        {
            existingRecord.Votes[imagePrediction.Prediction] += 1;
            await _db.SaveChangesAsync();
        }
        else
        {
            var newRecord = new NewImagePrediction(sha1, new Dictionary<Animal, int>()
            {
                {Animal.Cat, 0},
                {Animal.Dog, 0},
                {Animal.Neither, 0}
            });
            newRecord.Votes[imagePrediction.Prediction] += 1;
            await _db.AddAsync(newRecord);
            await _db.SaveChangesAsync();
        }
    }
}

public interface IDbService
{
    Task AddPrediction(ImagePrediction imagePrediction);
}
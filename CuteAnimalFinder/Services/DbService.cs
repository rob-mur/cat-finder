using System.Security.Cryptography;
using CuteAnimalFinder.Models;

namespace CuteAnimalFinder.Services;

public class DbService: IDbService
{

    private readonly IConfiguration _config;
    public DbService(IConfiguration config)
    {
        _config = config;
    }

    private async Task AddPredictionBackground(Models.ImagePrediction imagePrediction)
    {
        try
        {
            await using var ctx = new PredictionDbContext(_config);
            using var client = new HttpClient();
            using var response = await client.GetAsync(imagePrediction.Url);
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var sha1 = SHA1.HashData(imageBytes);
            var existingRecord = ctx.Predictions.FirstOrDefault(x => x.Sha1 == sha1);
            if (existingRecord != null)
            {
                existingRecord.Votes[imagePrediction.Prediction] += 1;
                await ctx.SaveChangesAsync();
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
                await ctx.AddAsync(newRecord);
                await ctx.SaveChangesAsync();
            }
        }
        catch (HttpRequestException e)
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
using CuteAnimalFinder.Models;
using CuteAnimalFinder.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Tweetinvi.Core.Extensions;

namespace CuteAnimalFinder.Services;

public class Prediction : IPrediction
{
    private readonly string _predictionUrl;
    private readonly IMediator _mediator;
    private readonly IConfiguration _config;

    public Prediction(IConfiguration config, IMediator mediator)
    {
        _config = config;
        _mediator = mediator;
        _predictionUrl = config.GetSection("PredictionURL").Value!;
    }

    public async Task<List<string>> FilterImages(Animal search, string[] images)
    {
        await using var dbContext = new PredictionDbContext(_config);
        var cachedPredictions = dbContext.Predictions.Where(x => images.Contains(x.Url)).ToArray().GroupBy(x => x.Url,
            (_, group) =>
            {
                var predictedImages = @group as ImagePrediction[] ?? @group.ToArray();
                var max = predictedImages.Max(x => x.Prediction);
                return predictedImages.First(x => x.Prediction == max);
            }).ToArray();

        await _mediator.Publish(new CacheNotification(cachedPredictions.Length));

        var unknownImages = images
            .Where(x => !cachedPredictions.Select(imagePrediction => imagePrediction.Url).Contains(x)).ToArray();
        await _mediator.Publish(new QueryApiNotification(unknownImages.Length));

        var result = await QueryPredictionApi(unknownImages);
        
        return result.IsEmpty()
            ? new List<string>()
            : result.Concat(cachedPredictions).Where(x => x.Prediction == search).Select(x => x.Url).ToList();
    }

    private async Task<ImagePrediction[]> QueryPredictionApi(string[] images)
    {
        var query = "?urls=" + string.Join("&urls=", images);
        using var client = new HttpClient();
        HttpResponseMessage response;
        try
        {
            response = client.GetAsync(new Uri(_predictionUrl + query)).Result;
        }
        catch (HttpRequestException e)
        {
            await _mediator.Publish(new PredictionQueryFailedNotification(e.Message));
            return Array.Empty<ImagePrediction>();
        }
        catch (AggregateException e)
        {
            await _mediator.Publish(new PredictionQueryFailedNotification(e.Message));
            return Array.Empty<ImagePrediction>();
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Animal[]>(responseString)!;
        return images.Zip(result).Select((x, _) => new ImagePrediction(x.First, x.Second)).ToArray();
    }
}

public interface IPrediction
{
    Task<List<string>> FilterImages(Animal search, string[] images);
}
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

    public async Task<Dictionary<string, bool>> FilterImages(Animal search, string[] images)
    {
        await using var dbContext = new PredictionDbContext(_config);
        var cachedPredictions = dbContext.Predictions.Where(x => images.Contains(x.Url)).ToArray().GroupBy(x => x.Url, (_, group) =>
            {
                var predictedImages = @group as PredictedImage[] ?? @group.ToArray();
                var max = predictedImages.Max(x => x.Prediction);
                return predictedImages.First(x => x.Prediction == max);
            })
            .ToDictionary(x => x!.Url, x => (Animal) x!.Prediction);
        var relevantCache = cachedPredictions.Where(x => x.Value == search).ToArray();
        await _mediator.Publish(new CacheNotification(relevantCache.Length));
        var unknownImages = images.Where(x => !cachedPredictions.ContainsKey(x)).ToArray();
        await _mediator.Publish(new QueryApiNotifcation(unknownImages.Length));
        var result = await QueryPredictionApi(unknownImages);
        if (result.IsEmpty())
            return new Dictionary<string, bool>();
        var relevantImages = unknownImages.Where((_, i) => (Animal) result[i] == search).ToArray();
        var filterResult = new Dictionary<string, bool>();
        foreach (var img in relevantCache)
            filterResult[img.Key] = true;
        foreach (var img in relevantImages)
            filterResult[img] = false;
        return filterResult;
    }

    private async Task<int[]> QueryPredictionApi(string[] images)
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
            return Array.Empty<int>();
        }
        catch (AggregateException e)
        {
            await _mediator.Publish(new PredictionQueryFailedNotification(e.Message));
            return Array.Empty<int>();
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<int[]>(responseString)!;
        return result;
    }
}

public interface IPrediction
{
    Task<Dictionary<string, bool>> FilterImages(Animal search, string[] images);
}
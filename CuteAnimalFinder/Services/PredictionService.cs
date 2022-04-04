using CuteAnimalFinder.Models;
using CuteAnimalFinder.Notifications;
using MediatR;
using Newtonsoft.Json;

namespace CuteAnimalFinder.Services;

public class Prediction : IPrediction
{
    private readonly IPredictionCache _cache;
    private readonly string _predictionUrl;
    private readonly IMediator _mediator;

    public Prediction(IConfiguration config, IPredictionCache cache, IMediator mediator)
    {
        _cache = cache;
        _mediator = mediator;
        _predictionUrl = config.GetSection("PredictionURL").Value!;
    }

    public async Task<Dictionary<string, bool>> FilterImages(Animal search, string[] images)
    {
        var cachedPredictions = await _cache.GetPredictions(images);
        var relevantCache = cachedPredictions.Where(x => x.Value == search).ToArray();
        await _mediator.Publish(new CacheNotification(relevantCache.Length));
        var unknownImages = images.Where(x => !cachedPredictions.ContainsKey(x)).ToArray();
        var result = await QueryPredictionApi(unknownImages);
        var relevantImages = unknownImages.Where((_,i) => (Animal)result[i] == search).ToArray();
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
        catch (TaskCanceledException e)
        {
            await _mediator.Publish(new PredictionQueryFailedNotification(e.Message));
            return Array.Empty<int>();
        }
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<int[]>(responseString)!;
        return result;
    }
}

public interface IPrediction
{
    Task<Dictionary<string, bool>> FilterImages(Animal search, string[] images);
}
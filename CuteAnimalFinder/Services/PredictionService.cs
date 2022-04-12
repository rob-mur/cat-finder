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
    private readonly IDbService _db;

    public Prediction(IConfiguration config, IMediator mediator, IDbService db)
    {
        _config = config;
        _mediator = mediator;
        _db = db;
        _predictionUrl = config.GetSection("PredictionURL").Value!;
    }

    public async Task<List<string>> FilterImages(Animal search, string[] images)
    {
        await _mediator.Publish(new QueryApiNotification(images.Length));

        var result = await QueryPredictionApi(images);
        
        return result.Where(x => x.Prediction == search).Select(x => x.Url).ToList()!;
    }

    private async Task<Models.ImagePrediction[]> QueryPredictionApi(string[] images)
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
            return Array.Empty<Models.ImagePrediction>();
        }
        catch (AggregateException e)
        {
            await _mediator.Publish(new PredictionQueryFailedNotification(e.Message));
            return Array.Empty<Models.ImagePrediction>();
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Animal[]>(responseString)!;
        return images.Zip(result).Select((x, _) => new Models.ImagePrediction(x.First, x.Second)).ToArray();
    }
}

public interface IPrediction
{
    Task<List<string>> FilterImages(Animal search, string[] images);
}
using CuteAnimalFinder.Models;
using Newtonsoft.Json;

namespace CuteAnimalFinder.Services;

public class Prediction : IPrediction
{
    private readonly IPredictionCache _cache;
    private readonly string _predictionUrl;

    public Prediction(IConfiguration config, IPredictionCache cache)
    {
        _cache = cache;
        _predictionUrl = config.GetSection("PredictionURL").Value!;
    }

    public Dictionary<string, bool> FilterImages(Animal search, string[] images)
    {
        var cachedPredictions = _cache.GetPredictions(images);
        var relevantCache = cachedPredictions.Where(x => x.Value == search).ToArray();
        var unknownImages = images.Where(x => !cachedPredictions.ContainsKey(x)).ToArray();
        var result = QueryPredictionApi(unknownImages);
        var relevantImages = unknownImages.Where((_,i) => (Animal)result[i] == search).ToArray();
        var filterResult = new Dictionary<string, bool>();
        foreach (var img in relevantCache)
            filterResult[img.Key] = true;
        foreach (var img in relevantImages)
            filterResult[img] = false;
        return filterResult;
    }

    private int[] QueryPredictionApi(string[] images)
    {
        var query = "?urls=" + string.Join("&urls=", images);
        using var client = new HttpClient();
        var response = client.GetAsync(new Uri(_predictionUrl + query)).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<int[]>(responseString)!;
        return result;
    }
}

public interface IPrediction
{
    Dictionary<string, bool> FilterImages(Animal search, string[] images);
}
using CuteAnimalFinder.Models;
using Newtonsoft.Json;

namespace CuteAnimalFinder.Services;

public class Prediction : IPrediction
{
    private readonly string _predictionUrl;
    public Prediction(IConfiguration config)
    {
        _predictionUrl = config.GetSection("PredictionURL").Value!;
    }
    public string[] FilterImages(Animal search, string[] images)
    {
        var query = "?urls=" + String.Join("&urls=", images);
        using var client = new HttpClient();
        var response = client.GetAsync(new Uri(_predictionUrl + query)).Result;
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<int[]>(responseString)!;
        return images.Where((x, i) => result[i] == (int) search).ToArray();
    }
}

public interface IPrediction
{
    string[] FilterImages(Animal search, string[] images);
}
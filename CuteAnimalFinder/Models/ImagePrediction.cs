namespace CuteAnimalFinder.Models;

public class ImagePrediction
{
    public ImagePrediction(string? url, Animal prediction)
    {
        Url = url;
        Prediction = prediction;
    }

    public string? Url { get;  }
    public Animal Prediction { get;  }
}
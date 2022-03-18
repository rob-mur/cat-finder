using System.Net;
using CuteAnimalFinder.Models;
using Keras;
using Keras.Models;

namespace CuteAnimalFinder.Services;

public class Prediction : IPrediction
{
    private readonly BaseModel _model;
    public Prediction()
    {
        Console.WriteLine("Initialising model");
        _model = BaseModel.LoadModel("cat_dog_neither.h5");
        Console.WriteLine("Model initialised");
    }
    public string[] FilterImages(Animal search, string[] images)
    {
        // TODO: this section should call a python FastAPI because the C~ Keras bindings suck ass
        string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        var filteredImages =  images.Where(image =>
        {
            // Download image
            var fileName = Path.Combine(tempDirectory, Path.GetRandomFileName());
            Console.WriteLine($"Downloading {fileName}");
#pragma warning disable CS0618
            using var client = new WebClient();
#pragma warning restore CS0618
            client.DownloadFile(image, fileName);
            Console.WriteLine("Finished downloading");
            // Run model on image
            var loadedImage = Keras.PreProcessing.Image.ImageUtil.LoadImg(fileName, target_size: new Shape(224, 224));
            var imageArray = Keras.PreProcessing.Image.ImageUtil.ImageToArray(loadedImage);
            // TODO: potentially add a section that centers the image based on the training mean used
            Console.WriteLine("Predicting begin");
            var output = _model.Predict(imageArray);
            Console.WriteLine("Predicting end");
            output = output[0];
            // Check if prediction matches search
            Console.WriteLine($"Checking that {output} is equal to {search}");
            return output == search;
        }).ToArray();
        Directory.Delete(tempDirectory,true);
        return filteredImages;
    }
}

public interface IPrediction
{
    string[] FilterImages(Animal search, string[] images);
}
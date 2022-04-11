namespace CuteAnimalFinder.Services;

public class DbService: IDbService
{
    private readonly PredictionDbContext _db;
    public DbService(IConfiguration config)
    {
        _db = new PredictionDbContext(config);
    }
    public Task AddPrediction(ImagePrediction imagePrediction)
    {
        _db.Predictions.Add(imagePrediction);
        return Task.CompletedTask;
    }
}

public interface IDbService
{
    Task AddPrediction(ImagePrediction imagePrediction);
}
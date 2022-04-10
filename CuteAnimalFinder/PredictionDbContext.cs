using Microsoft.EntityFrameworkCore;

namespace CuteAnimalFinder;

public class PredictionDbContext :  DbContext
{
    private readonly string _connectionString;

    public DbSet<PredictedImage> Predictions { get; set; } = null!;

    public PredictionDbContext(IConfiguration config)
    {
        _connectionString = config.GetSection("PredictionCacheCS").Value!;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
        
    }
}

[Keyless]
public  class PredictedImage
{
    public string Url { get; init; } = null!;
    public int Prediction { get; init; }
}
using System.ComponentModel.DataAnnotations;
using CuteAnimalFinder.Models;
using Microsoft.EntityFrameworkCore;

namespace CuteAnimalFinder;

public class PredictionDbContext :  DbContext
{
    private readonly string _connectionString;

    public DbSet<ImagePrediction> Predictions { get; set; } = null!;

    public PredictionDbContext(IConfiguration config)
    {
        _connectionString = config.GetSection("PredictionCacheCS").Value!;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
        
    }
}

public class ImagePrediction
{
    [Key]
    public string Url { get; init; }
    public Animal Prediction { get; init; }

    public ImagePrediction(string url, Animal prediction)
    {
        Url = url;
        Prediction = prediction;
    }
}
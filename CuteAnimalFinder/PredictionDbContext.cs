using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using CuteAnimalFinder.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CuteAnimalFinder;

public class PredictionDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<ImagePrediction> Predictions { get; set; } = null!;
    public DbSet<NewImagePrediction> NewPredictions { get; set; } = null!;

    public PredictionDbContext(IConfiguration config)
    {
        _connectionString = config.GetSection("PredictionCacheCS").Value!;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NewImagePrediction>().Property(b => b.Votes).HasConversion(
            v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<Dictionary<Animal, int>>(v));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }

    private async Task AddPrediction(ImagePrediction prediction)
    {
        Predictions.Add(prediction);
    }
}

public class ImagePrediction
{
    [Key] public string Url { get; init; }
    public Animal Prediction { get; init; }

    public ImagePrediction(string url, Animal prediction)
    {
        Url = url;
        Prediction = prediction;
    }
}

public class NewImagePrediction
{
    public NewImagePrediction(byte[] sha1, Dictionary<Animal, int> votes)
    {
        Sha1 = sha1;
        Votes = votes;
    }

    [Key] public byte[] Sha1 { get; init; }

    public Dictionary<Animal, int> Votes { get; init; }
}
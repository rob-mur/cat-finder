using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using CuteAnimalFinder.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace CuteAnimalFinder;

public class PredictionDbContext : DbContext
{
    public DbSet<ImagePrediction> Predictions { get; init; } = null!;

    public PredictionDbContext(DbContextOptions<PredictionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImagePrediction>().Property(b => b.Votes).HasConversion(
            v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<Dictionary<Animal, int>>(v),
            ValueComparer.CreateDefault(typeof(Dictionary<Animal, int>), true));
    }
}

public class ImagePrediction
{
    public ImagePrediction(byte[] sha1, Dictionary<Animal, int> votes, string exampleUrl)
    {
        Sha1 = sha1;
        Votes = votes;
        ExampleUrl = exampleUrl;
    }

    [Key] public byte[] Sha1 { get; init; }

    public Dictionary<Animal, int> Votes { get; init; }

    public string ExampleUrl { get; init; }
}
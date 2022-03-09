using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace CuteAnimalFinder.Settings;

public class TwitterTokens
{
    public string? ConsumerToken { get; set; }
    public string? ConsumerSecret { get; set; }
    public string? AccessToken { get; set; }
    public string? AccessSecret { get; set; }
}
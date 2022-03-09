using CuteAnimalFinder.Settings;
using Microsoft.AspNetCore.Components;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models.V2;

namespace CuteAnimalFinder.Services;

public class Sources : ISources
{
    private readonly TwitterClient _client;
    public Sources(IConfiguration config)
    {
        var twitterTokens = config.GetSection("TwitterTokens").Get<TwitterTokens>();
        _client = new TwitterClient(twitterTokens.ConsumerToken, twitterTokens.ConsumerSecret,
            twitterTokens.AccessToken, twitterTokens.AccessSecret);
        Console.WriteLine();
    }

    public async Task<string[]> GetLatestPictures(string search)
    {
        SearchTweetsV2Response response;
        try
        {
            response = await _client.SearchV2.SearchTweetsAsync($"has:images {search}");
        }
        catch (Exception)
        {
            return new string[]{};
        }
        var potentiallySensitiveMediaKeys = response.Tweets.Where(x => x.PossiblySensitive)
            .SelectMany(x => x.Attachments.MediaKeys).ToArray();
        return response.Includes.Media
            .Where(x => x.Type == "photo" && !(potentiallySensitiveMediaKeys.Length > 0 && potentiallySensitiveMediaKeys.Contains(x.MediaKey))).Select(x => x.Url)
            .ToArray();
    }
}

public interface ISources
{
    Task<string[]> GetLatestPictures(string search);
}
using CuteAnimalFinder.Models;
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
    }

    public async Task<string[]> GetLatestPictures(Animal search)
    {
        SearchTweetsV2Response response;
        try
        {
            response = await _client.SearchV2.SearchTweetsAsync($"has:images {search.ToString()}");
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }

        var sensitiveTweets = response.Tweets.Where(x => x.PossiblySensitive).ToArray();
        if (sensitiveTweets.Length == 0)
            return response.Includes.Media.Where(x => x.Type == "photo").Select(x => x.Url).ToArray();
        var sensitiveMediaKeys = sensitiveTweets.Where(x=>x.Attachments != null).Where(x => x.Attachments.MediaKeys != null).SelectMany(x => x.Attachments.MediaKeys);
        return response.Includes.Media
            .Where(x => x.Type == "photo" && !sensitiveMediaKeys.Contains(x.MediaKey)).Select(x => x.Url)
            .ToArray();
    }
}

public interface ISources
{
    Task<string[]> GetLatestPictures(Animal search);
}
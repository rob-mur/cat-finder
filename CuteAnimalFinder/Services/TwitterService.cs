using CuteAnimalFinder.Models;
using CuteAnimalFinder.Notifications;
using CuteAnimalFinder.Settings;
using MediatR;
using Tweetinvi;
using Tweetinvi.Models.V2;

namespace CuteAnimalFinder.Services;

public class TwitterService : ISources
{
    private readonly TwitterClient _client;
    private readonly IMediator _mediator;
    public TwitterService(IConfiguration config, IMediator mediator)
    {
        _mediator = mediator;
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
        catch (Exception e)
        {
            await _mediator.Publish(new TwitterErrorNotification(e.Message));
            return Array.Empty<string>();
        }

        var sensitiveTweets = response.Tweets.Where(x => x.PossiblySensitive).ToArray();
        if (sensitiveTweets.Length == 0)
            return response.Includes.Media.Where(x => x.Type == "photo").Select(x => x.Url).ToArray();
        var sensitiveMediaKeys = sensitiveTweets.Where(x => x.Attachments != null)
            .Where(x => x.Attachments.MediaKeys != null).SelectMany(x => x.Attachments.MediaKeys);
        var results =  response.Includes.Media
            .Where(x => x.Type == "photo" && !sensitiveMediaKeys.Contains(x.MediaKey)).Select(x => x.Url)
            .ToArray();
        await _mediator.Publish(new TwitterSuccessNotification(results.Length));
        return results;
    }
}

public interface ISources
{
    Task<string[]> GetLatestPictures(Animal search);
}

using TwitterSharp.Client;
using TwitterSharp.Request;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Response.RMedia;
using TwitterSharp.Response.RTweet;
using TwitterSharp.Rule;

namespace CatFinder.Server.Services;

public class Sources
{
    private readonly TwitterClient _client;
    public Sources()
    {
        _client = new TwitterSharp.Client.TwitterClient("AAAAAAAAAAAAAAAAAAAAAOIFaAEAAAAASR8x255u%2FCUOYC5zP7MsO7Am%2BcU%3DHAMGkBeeHuBnprrkAzlyKanCCHW4Jr6KDlGZLniDGcgsGyNATY");

    }
    public async void GetLatestPictures()
    {
        var streamInfo = _client.AddTweetStreamAsync(new StreamRequest(Expression.HasImages(),"cat"));
        var onTweetReceived = ScrapeMediaFromTweet;
        await _client.NextTweetStreamAsync(onTweetReceived, mediaOptions: new MediaOption[] {MediaOption.Url});
    }

    private void ScrapeMediaFromTweet(Tweet tweet)
    {
        var images = tweet.Attachments.Media.Where(x => x.Type == MediaType.Photo).Select(x => x.Url).ToArray();
    }
}
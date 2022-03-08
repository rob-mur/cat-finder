using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models.V2;

namespace CuteAnimalFinder.Services;

public class Sources
{
    private readonly TwitterClient _client;

    public Sources()
    {
        _client = new TwitterClient("GvNyNOyBNwB0mWx4R51FlSXPA", "pQrYMK5QT7XwtIeBWgCwOnuor7TnO5MekmrATRNUjx1NtogZhT",
            "846023882-mbroGI8PWBBQYYOE10qXcgoVazzl63kVtKki00JF", "cUeOAfbv6dcttLvadLt1o02rQDHGP0OwQe9kNYSWBehO0");
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
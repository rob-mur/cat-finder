using Tweetinvi;

namespace CatFinder.Server.Services;

public class Sources
{
    private readonly TwitterClient _client;

    public Sources()
    {
        _client = new TwitterClient("GvNyNOyBNwB0mWx4R51FlSXPA", "pQrYMK5QT7XwtIeBWgCwOnuor7TnO5MekmrATRNUjx1NtogZhT",
            "846023882-mbroGI8PWBBQYYOE10qXcgoVazzl63kVtKki00JF", "cUeOAfbv6dcttLvadLt1o02rQDHGP0OwQe9kNYSWBehO0");
    }

    public async Task<string[]> GetLatestPictures()
    {
        var response = await _client.SearchV2.SearchTweetsAsync("has:images cat");
        return response.Includes.Media.Where(x => x.Type == "photo").Select(x => x.Url).ToArray();
    }
}
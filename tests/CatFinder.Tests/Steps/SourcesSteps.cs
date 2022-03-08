using CatFinder.Server.Services;
using NUnit.Framework;

namespace CatFinder.Tests.Steps;

[Binding]
public class SourcesSteps
{
    private object[] _pictures= Array.Empty<object>();
    private readonly Sources _sources;

    public SourcesSteps(Sources sources)
    {
        _sources = sources;
    }

    [When(@"the Sources are asked for their latest (.*) pictures")]
    public void WhenTheSourcesAreAskedForTheirLatestPictures(int numberOfPictures)
    {
        _pictures = _sources.GetLatestPictures(numberOfPictures);
    }

    [Then(@"the latest (.*) pictures are returned")]
    public void ThenTheLatestPicturesAreReturned(int numberOfPictures)
    {
        Assert.AreEqual(numberOfPictures, _pictures.Length);
    }
}
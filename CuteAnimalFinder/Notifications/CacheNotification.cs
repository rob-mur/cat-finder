using MediatR;

namespace CuteAnimalFinder.Notifications;

public class CacheNotification : INotification
{
    public CacheNotification(int count)
    {
        Count = count;
    }

    public int Count { get; }
}
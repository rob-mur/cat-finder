using MediatR;

namespace CuteAnimalFinder.Notifications;

public class QueryApiNotification : INotification
{
    public QueryApiNotification(int count)
    {
        Count = count;
    }

    public int Count { get; }
}
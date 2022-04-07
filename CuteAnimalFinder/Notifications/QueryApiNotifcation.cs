using MediatR;

namespace CuteAnimalFinder.Notifications;

public class QueryApiNotifcation : INotification
{
    public QueryApiNotifcation(int count)
    {
        Count = count;
    }

    public int Count { get; }
}
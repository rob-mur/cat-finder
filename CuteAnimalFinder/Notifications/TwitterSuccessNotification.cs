using MediatR;

namespace CuteAnimalFinder.Notifications;

public class TwitterSuccessNotification : INotification
{
    public TwitterSuccessNotification(int count)
    {
        Count = count;
    }

    public int Count { get; }
}
using MediatR;

namespace CuteAnimalFinder.Notifications;

public class TwitterErrorNotification : INotification
{
    public TwitterErrorNotification(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
using MediatR;

namespace CuteAnimalFinder.Notifications;

public class PredictionQueryFailedNotification : INotification
{
    public PredictionQueryFailedNotification(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
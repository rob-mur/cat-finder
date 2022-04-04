using MediatR;

namespace CuteAnimalFinder.Notifications;

public class SqlCrashNotification : INotification
{
    public SqlCrashNotification(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
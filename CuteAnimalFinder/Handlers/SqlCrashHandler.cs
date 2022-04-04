using CuteAnimalFinder.Notifications;
using CuteAnimalFinder.Services;
using MediatR;

namespace CuteAnimalFinder.Handlers;

public class SqlCrashHandler : INotificationHandler<SqlCrashNotification >
{
    private readonly IComponentStateService _state;

    public SqlCrashHandler (IComponentStateService state)
    {
        _state = state;
    }

    public Task Handle(SqlCrashNotification    notification, CancellationToken cancellationToken)
    {
        _state.ErrorQueue.Messages.Add("Failed to read the database. Most likely it's still warming up, try refreshing.");
        _state.ErrorQueue.Messages.Add($"Full error: {notification.Message}");
        _state.ErrorQueue.Refresh();
        return Task.CompletedTask;
    }
}
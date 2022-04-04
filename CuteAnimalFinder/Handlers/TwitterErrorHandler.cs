using CuteAnimalFinder.Notifications;
using CuteAnimalFinder.Services;
using MediatR;

namespace CuteAnimalFinder.Handlers;

public class TwitterErrorHandler : INotificationHandler<TwitterErrorNotification >
{
    private readonly IComponentStateService _state;

    public TwitterErrorHandler(IComponentStateService state)
    {
        _state = state;
    }

    public Task Handle(TwitterErrorNotification  notification, CancellationToken cancellationToken)
    {
        _state.ErrorQueue.Messages.Add($"Couldn't connect to twitter. Detailed error: {notification.Message}");
        _state.ErrorQueue.Refresh();
        return Task.CompletedTask;
    }
}
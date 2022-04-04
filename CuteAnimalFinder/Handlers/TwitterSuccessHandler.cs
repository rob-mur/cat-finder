using CuteAnimalFinder.Notifications;
using CuteAnimalFinder.Services;
using MediatR;

namespace CuteAnimalFinder.Handlers;

public class TwitterSuccessHandler : INotificationHandler<TwitterSuccessNotification>
{
    private readonly IComponentStateService _state;

    public TwitterSuccessHandler (IComponentStateService state)
    {
        _state = state;
    }

    public Task Handle(TwitterSuccessNotification   notification, CancellationToken cancellationToken)
    {
        _state.ErrorQueue.Messages.Add($"Successfully connected to twitter. Found {notification.Count} relevant images");
        _state.ErrorQueue.Refresh();
        return Task.CompletedTask;
    }
}
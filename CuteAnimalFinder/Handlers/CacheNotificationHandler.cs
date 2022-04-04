using CuteAnimalFinder.Notifications;
using CuteAnimalFinder.Services;
using MediatR;

namespace CuteAnimalFinder.Handlers;

public class CacheNotificationHandler : INotificationHandler<CacheNotification>
{
    private readonly IComponentStateService _state;

    public CacheNotificationHandler(IComponentStateService state)
    {
        _state = state;
    }

    public Task Handle(CacheNotification notification, CancellationToken cancellationToken)
    {
        _state.ErrorQueue.Messages.Add($"Found {notification.Count} predictions from cache");
        _state.ErrorQueue.Refresh();
        return Task.CompletedTask;
    }
}
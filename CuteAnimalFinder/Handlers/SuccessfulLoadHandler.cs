using CuteAnimalFinder.Notifications;
using CuteAnimalFinder.Services;
using MediatR;

namespace CuteAnimalFinder.Handlers;

public class SuccessfulLoadHandler : INotificationHandler<SuccessfulLoadNotification >
{
    private readonly IComponentStateService _state;

    public SuccessfulLoadHandler (IComponentStateService state)
    {
        _state = state;
    }

    public Task Handle(SuccessfulLoadNotification notification, CancellationToken cancellationToken)
    {
        _state.ErrorQueue.Messages.Clear();
        _state.ErrorQueue.Refresh();
        return Task.CompletedTask;
    }
}
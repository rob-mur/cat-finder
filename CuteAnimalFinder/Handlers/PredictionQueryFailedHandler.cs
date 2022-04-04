using CuteAnimalFinder.Notifications;
using CuteAnimalFinder.Services;
using MediatR;

namespace CuteAnimalFinder.Handlers;

public class PredictionQueryFailedHandler : INotificationHandler<PredictionQueryFailedNotification >
{
    private readonly IComponentStateService _state;

    public PredictionQueryFailedHandler (IComponentStateService state)
    {
        _state = state;
    }

    public Task Handle(PredictionQueryFailedNotification    notification, CancellationToken cancellationToken)
    {
        _state.ErrorQueue.Messages.Add("Failed to access the deep learning API. Try refreshing.");
        _state.ErrorQueue.Messages.Add($"Full error: {notification.Message}");
        _state.ErrorQueue.Refresh();
        return Task.CompletedTask;
    }
}
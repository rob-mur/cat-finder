using CuteAnimalFinder.Notifications;
using CuteAnimalFinder.Services;
using MediatR;

namespace CuteAnimalFinder.Handlers;

public class QueryApiHandler : INotificationHandler<QueryApiNotifcation>
{
    private readonly IComponentStateService _state;

    public QueryApiHandler(IComponentStateService state)
    {
        _state = state;
    }

    public Task Handle(QueryApiNotifcation notification, CancellationToken cancellationToken)
    {
        _state.ErrorQueue.Messages.Add($"Querying the deep learning API for {notification.Count} images");
        _state.ErrorQueue.Refresh();
        return Task.CompletedTask;
    }
}
using CuteAnimalFinder.Components;
using CuteAnimalFinder.Components.PictureStream;

namespace CuteAnimalFinder.Services;

public class ComponentStateService : IComponentStateService
{
    public ErrorQueue ErrorQueue { get; set; } = null!;
}

public interface IComponentStateService
{
    ErrorQueue ErrorQueue { get; set; }
}
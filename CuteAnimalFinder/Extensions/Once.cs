namespace CuteAnimalFinder.Extensions;

public class Once<T>
{
    private readonly Func<T,Task> _work;
    private readonly List<T> _seenArgs = new();
    
    public Once(Func<T,Task> work)
    {
        _work = work;
    }

    public async Task Invoke(T args)
    {
        if (_seenArgs.Contains(args))
            return;
        _seenArgs.Add(args);
        await _work.Invoke(args);
    }
}
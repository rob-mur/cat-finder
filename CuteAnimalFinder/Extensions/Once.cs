namespace CuteAnimalFinder.Extensions;

public class Once<T>
{
    private readonly Action<T> _work;
    private readonly List<T> _seenArgs = new();
    
    public Once(Action<T> work)
    {
        _work = work;
    }

    public void Invoke(T args)
    {
        if (_seenArgs.Contains(args))
            return;
        _seenArgs.Add(args);
        _work.Invoke(args);
    }
}
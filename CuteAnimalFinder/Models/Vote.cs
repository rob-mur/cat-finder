namespace CuteAnimalFinder.Models;

public class Vote<T>
{
    public bool HasVoted { get; private set; } = false;
    public T Value { get; }
    public Vote(T value)
    {
        Value = value;
    }

    public void Cast()
    {
        HasVoted = true;
    }
}
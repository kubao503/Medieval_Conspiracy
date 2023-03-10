using System.Collections.Generic;


public class Subject
{
    private List<IObserver> _observers = new();

    public void AddObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void Notify()
    {
        foreach (var observer in _observers)
            observer.Notify();
    }
}

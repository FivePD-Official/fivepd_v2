using System;
using System.Collections.Generic;
using FivePD.Server.Interfaces.VariableData;

namespace FivePD.Server.Models.VariableData;

/// <summary>
/// Represents a dynamic entity observable.
/// </summary>
public class VariableDataEntityObservable
{
    private readonly List<Action<VariableDataEvent>> _observers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDataEntityObservable"/> class.
    /// </summary>
    /// <param name="setType">The set's type to use for invoking the subscribers.</param>
    /// <param name="type">The type of the action to subscribe.</param>
    public VariableDataEntityObservable(Type setType, VariableDataEntityObservableType type)
    {
        this.SetType = setType;
        this.Type = type;
    }

    /// <summary>
    /// Gets the type of the set.
    /// </summary>
    public Type SetType { get; }

    /// <summary>
    /// Gets the type of the subscribed action.
    /// </summary>
    public VariableDataEntityObservableType Type { get; }

    /// <summary>
    /// Subscribes to the given action of a set.
    /// </summary>
    /// <param name="observer">The action which will be invoked.</param>
    /// <returns>A disposable <see cref="VariableDataEntityObservableSubscription"/>.</returns>
    public IDisposable Subscribe(Action<VariableDataEvent> observer)
    {
        this._observers.Add(observer);
        return new VariableDataEntityObservableSubscription(this, observer);
    }

    /// <summary>
    /// Unsubscribes the given observer.
    /// </summary>
    /// <param name="action">The action to unsubscribe.</param>
    public void Unsubscribe(Action<VariableDataEvent> action)
    {
        this._observers.Remove(action);
    }

    /// <summary>
    /// Invokes all observers.
    /// </summary>
    /// <param name="event">The entity to invoke the action with.</param>
    public void InvokeObservers(VariableDataEvent @event)
    {
        foreach (var o in this._observers)
        {
            o?.Invoke(@event);
        }
    }
}
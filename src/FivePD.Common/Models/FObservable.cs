using System;
using System.Collections.Generic;
using CitizenFX.Core;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Common.Models;

/// <summary>
/// Represents a single stateful datum. A property defined with
/// this type will be automatically persisted
/// assuming the property's class is of the
/// <see cref="FStatefulContainer"/> type.
/// </summary>
/// <typeparam name="T">The serializable type
/// being persisted.</typeparam>
#pragma warning disable SA1649
public class FObservable<T> : IDisposable
#pragma warning restore SA1649
{
    private readonly List<Action<T>> _observers = new();
    private bool _disposed;
    private T _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="FObservable{T}"/> class.
    /// </summary>
    /// <param name="defaultValue">The value to set, if undefined.</param>
    public FObservable(T defaultValue)
    {
        this._value = defaultValue;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="FObservable{T}"/> class.
    /// </summary>
    ~FObservable() => this.Dispose(false);

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public T Value
    {
        get
        {
            var value = this._value;
            if (value is null && typeof(T).IsValueType)
            {
                return default;
            }

            return value;
        }

        set
        {
            var currentVal = this.Value;

            // If the currentVal and the new value are the same, do nothing.
            if (currentVal != null && currentVal.Equals(value))
            {
                return;
            }

            // Set the value in the state bag and notify observers.
            this._value = value;
            this.NotifyObservers(value);
        }
    }

    /// <summary>
    /// Observes the current value and calls the
    /// specified action when it changes. This
    /// method will provide the current value
    /// immediately when subscribed. Disposing
    /// of the returned <see cref="IDisposable"/>
    /// will unsubscribe the observer.</summary>
    /// <param name="observer">The action to subscribe.</param>
    /// <param name="emitLastEventOnSubscription">Sets whether the last event should be emitted on subscription.</param>
    /// <returns>An <see cref="IDisposable"/> to
    /// use to unsubscribe.</returns>
    public IDisposable Subscribe(Action<T> observer, bool emitLastEventOnSubscription = true)
    {
        this._observers.Add(observer);

        if (emitLastEventOnSubscription)
        {
            observer.Invoke(this.Value);
        }

        return new FSObservableSubscription<T>(this, observer);
    }

    /// <summary>
    /// Unsubscribes the given observer.
    /// </summary>
    /// <param name="action">The action to unsubscribe.</param>
    public void Unsubscribe(Action<T> action)
    {
        // Removes the given action from the list of observers.
        this._observers.Remove(action);

        Debug.WriteLine("removed...");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the observable.
    /// </summary>
    /// <param name="disposing">If we're disposing
    /// manually.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Remove all observers.
            this._observers.Clear();
        }

        if (!this._disposed)
        {
            this._disposed = true;
        }
    }

    private void NotifyObservers(T value)
    {
        foreach (var observer in this._observers)
        {
            observer.Invoke(value);
        }
    }
}
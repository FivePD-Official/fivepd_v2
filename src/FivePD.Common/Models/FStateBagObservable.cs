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
public class FStateBagObservable<T> : IDisposable
#pragma warning restore SA1649
{
    private readonly string _containerName;
    private readonly StateBag _stateBag;
    private readonly string _key;
    private readonly List<Action<T>> _observers = new();

#if !IS_GAMEMODE_SERVER
    private int _stateBagChangeHandler;
#endif

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FStateBagObservable{T}"/> class.
    /// </summary>
    /// <param name="container">The item's state container.</param>
    /// <param name="key">The key to use for the item's state.</param>
    /// <param name="defaultValue">The value to set, if undefined.</param>
    public FStateBagObservable(object container, string key, T defaultValue)
    {
        this._containerName = ((FStatefulContainer)container).GetStateKey();
        this._stateBag = ((FStatefulContainer)container).GetStateBag();
        this._key = key;

        if (this._stateBag[this._key] == null)
        {
            this._stateBag[this._key] = defaultValue;
        }

        this.ListenForStateChanges();
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="FStateBagObservable{T}"/> class.
    /// </summary>
    ~FStateBagObservable() => this.Dispose(false);

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public T Value
    {
        get
        {
            var value = this._stateBag[this._key];
            if (value is null && typeof(T).IsValueType)
            {
                return default;
            }

            return (T)value;
        }

        set
        {
            // Can (should) only set values from the server.
#if !IS_GAMEMODE_SERVER
            throw new InvalidOperationException("Cannot set the value of an observable from the client. Instead, send a command to the server to set the value.");
#else
                var currentVal = this.Value;

                // If the currentVal and the new value are the same, do nothing.
                if (currentVal != null && currentVal.Equals(value))
                {
                    return;
                }

                // Set the value in the state bag and notify observers.
                this._stateBag[this._key] = value;
                this.NotifyObservers(value);
#endif
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

        return new FStateBagObservableSubscription<T>(this, observer);
    }

    /// <summary>
    /// Unsubscribes the given observer.
    /// </summary>
    /// <param name="action">The action to unsubscribe.</param>
    public void Unsubscribe(Action<T> action)
    {
        // Removes the given action from the list of observers.
        this._observers.Remove(action);
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
        // If we're manually disposing...
        if (disposing)
        {
            // Remove all observers.
            this._observers.Clear();
        }

        // Check to see if Dispose has already been called.
        if (!this._disposed)
        {
            // Clean up state bag handler.
#if !IS_GAMEMODE_SERVER
            if (this._stateBagChangeHandler != 0)
            {
                Cfx.API.RemoveStateBagChangeHandler(this._stateBagChangeHandler);
            }
#endif

            // Note disposing has been done.
            this._disposed = true;
        }
    }

    private void ListenForStateChanges()
    {
        // Client only.
#if !IS_GAMEMODE_SERVER
        // On the client, we need to listen for state changes from the server.
        this._stateBagChangeHandler = Cfx.API.AddStateBagChangeHandler(
            this._key,
            this._containerName,
            new Action<string, string, dynamic, dynamic, bool>((bagName, stateKey, value, unused, replicated) =>
            {
                value = (T)value;

                // Discard null values.
                if (value is null)
                {
                    return;
                }

                this.NotifyObservers(value);
            }));
#endif
    }

    private void NotifyObservers(T value)
    {
        foreach (var observer in this._observers)
        {
            observer.Invoke(value);
        }
    }
}
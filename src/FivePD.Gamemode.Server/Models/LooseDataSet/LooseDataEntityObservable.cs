using System;
using System.Collections.Generic;

namespace FivePD.Gamemode.Server.Models.LooseDataSet
{
    /// <summary>
    /// Represents a dynamic entity observable.
    /// </summary>
    public class LooseDataEntityObservable
    {
        private readonly List<Action<LooseDataEntity>> _observers = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="LooseDataEntityObservable"/> class.
        /// </summary>
        /// <param name="setType">The set's type to use for invoking the subscribers.</param>
        /// <param name="type">The type of the action to subscribe.</param>
        public LooseDataEntityObservable(Type setType, LooseDataEntityObservableType type)
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
        public LooseDataEntityObservableType Type { get; }

        /// <summary>
        /// Subscribes to the given action of a set.
        /// </summary>
        /// <param name="observer">The action which will be invoked.</param>
        /// <returns>A disposable <see cref="LooseDataEntityObservableSubscription"/>.</returns>
        public IDisposable Subscribe(Action<LooseDataEntity> observer)
        {
            this._observers.Add(observer);
            return new LooseDataEntityObservableSubscription(this, observer);
        }

        /// <summary>
        /// Unsubscribes the given observer.
        /// </summary>
        /// <param name="action">The action to unsubscribe.</param>
        public void Unsubscribe(Action<LooseDataEntity> action)
        {
            this._observers.Remove(action);
        }

        /// <summary>
        /// Invokes all observers.
        /// </summary>
        /// <param name="entity">The entity to invoke the action with.</param>
        public void InvokeObservers(LooseDataEntity entity)
        {
            foreach (var o in this._observers)
            {
                o?.Invoke(entity);
            }
        }
    }
}
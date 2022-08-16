// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common.Attributes;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Common.Models
{
    /// <summary>
    /// A stateful item that can
    /// be observed.
    /// </summary>
    /// <typeparam name="T">The type of data to watch.</typeparam>
    #pragma warning disable SA1649
    public class FObservable<T> : IDisposable
    #pragma warning restore SA1649
    {
        private readonly string _containerName;
        private readonly StateBag _stateBag;
        private readonly string _key;
        private readonly List<Action<T>> _observers = new List<Action<T>>();

        #if !IS_GAMEMODE_SERVER
        private int _stateBagChangeHandler;
        #endif

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FObservable{T}"/> class.
        /// </summary>
        /// <param name="container">The item's state container.</param>
        /// <param name="key">The key to use for the item's state.</param>
        /// <param name="defaultValue">The value to set, if undefined.</param>
        public FObservable(object container, string key, T defaultValue)
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
        /// Finalizes an instance of the FObservable{T} class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1643:DestructorSummaryDocumentationMustBeginWithStandardText", Justification = "Reviewed.")]
        ~FObservable()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public T Value
        {
            get => (T)this._stateBag[this._key];

            set
            {
                var currentVal = this.Value;

                // If the currentVal and the new value are the same, do nothing.
                if (currentVal != null && currentVal.Equals(value))
                {
                    return;
                }

                // Set the value in the state bag and notify observers.
                this._stateBag[this._key] = value;
                this.NotifyObservers(value);
            }
        }

        /// <summary>
        /// Subscribe to changes.
        /// </summary>
        /// <param name="observer">The action to subscribe.</param>
        /// <returns>Dispose to unsub.</returns>
        public IDisposable Subscribe(Action<T> observer)
        {
            this._observers.Add(observer);

            // Updates the new observer with the current value.
            observer.Invoke(this.Value);

            return null;
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
        /// <param name="disposing">If we're disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // Clean up state bag handler.
                #if !IS_GAMEMODE_SERVER
                if (this._stateBagChangeHandler != 0)
                {
                    Debug.WriteLine("Cleaning up handler");
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
}
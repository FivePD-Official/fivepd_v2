using System;

namespace FivePD.Common.Models
{
    /// <summary>
    /// A subscription to a <see cref="FObservable{T}"/>.
    /// Utilize the <see cref="Dispose"/> method to
    /// unsubscribe.
    /// <typeparam name="T">The type being watched.</typeparam>
    /// </summary>
    #pragma warning disable SA1649
    #pragma warning disable SA1618
    #pragma warning disable S3881 // Conforms to pattern.
    public class FObservableSubscription<T> : IDisposable
    {
        private readonly Action<T> _action;
        private FObservable<T> _subject;

        /// <summary>
        /// Initializes a new instance of the <see cref="FObservableSubscription{T}"/> class.
        /// </summary>
        /// <param name="subject">The <see cref="FObservable{T}"/>.</param>
        /// <param name="action">The specific subscription to unsubscribe.</param>
        public FObservableSubscription(FObservable<T> subject, Action<T> action)
        {
            this._subject = subject;
            this._action = action;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="FObservable{T}"/>.
        /// </summary>
        public void Dispose()
        {
            this._subject.Unsubscribe(this._action);
            this._subject = null!;
        }
    }
}
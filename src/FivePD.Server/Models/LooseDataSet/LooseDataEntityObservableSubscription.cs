using System;

namespace FivePD.Server.Models.LooseDataSet;

/// <summary>
/// Represents a dynamic entity observable subscription.
/// </summary>
#pragma warning disable S3881 // Conforms to pattern.
public class LooseDataEntityObservableSubscription : IDisposable
{
    private readonly Action<LooseDataEntity> _action;
    private LooseDataEntityObservable _subject;

    /// <summary>
    /// Initializes a new instance of the <see cref="LooseDataEntityObservableSubscription"/> class.
    /// </summary>
    /// <param name="subject">The <see cref="LooseDataEntityObservable"/> to use.</param>
    /// <param name="action">The action to unsubscribe.</param>
    public LooseDataEntityObservableSubscription(LooseDataEntityObservable subject, Action<LooseDataEntity> action)
    {
        this._subject = subject;
        this._action = action;
    }

    /// <summary>
    /// Unsubscribes from the <see cref="LooseDataEntityObservable"/>.
    /// </summary>
    public void Dispose()
    {
        this._subject.Unsubscribe(this._action);
        this._subject = null!;
    }
}
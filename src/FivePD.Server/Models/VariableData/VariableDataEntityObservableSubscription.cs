using System;
using FivePD.Server.Interfaces.VariableData;

namespace FivePD.Server.Models.VariableData;

/// <summary>
/// Represents a dynamic entity observable subscription.
/// </summary>
#pragma warning disable S3881
public class VariableDataEntityObservableSubscription : IDisposable
{
    private readonly Action<VariableDataEvent> _action;
    private VariableDataEntityObservable _subject;

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDataEntityObservableSubscription"/> class.
    /// </summary>
    /// <param name="subject">The <see cref="VariableDataEntityObservable"/> to use.</param>
    /// <param name="action">The action to unsubscribe.</param>
    public VariableDataEntityObservableSubscription(VariableDataEntityObservable subject, Action<VariableDataEvent> action)
    {
        this._subject = subject;
        this._action = action;
    }

    /// <summary>
    /// Unsubscribes from the <see cref="VariableDataEntityObservable"/>.
    /// </summary>
    public void Dispose()
    {
        this._subject.Unsubscribe(this._action);
        this._subject = null!;
    }
}
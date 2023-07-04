using System.Threading;
using System.Threading.Tasks;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Models.AssistanceServiceModels;

#pragma warning disable

public abstract class AssistanceService : IInternalAssistanceService
{
    private FPlayer _requestedBy;

    public FPlayer RequestedBy => this._requestedBy;

    FPlayer IInternalAssistanceService.RequestedBy
    {
        get => this._requestedBy;
        set => this._requestedBy = value;
    }

    public abstract Task Setup();

    public abstract Task OnCalledAsync(CancellationTokenSource cancellationTokenSource);

    public abstract Task OnCancellationRequestedAsync();

    public abstract Task OnCompletedAsync();
}

internal interface IInternalAssistanceService
{
    internal FPlayer RequestedBy { get; set; }
}
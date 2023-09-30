using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Models.AssistanceServiceModels;

#pragma warning disable

public abstract class AssistanceService : IInternalAssistanceService
{
    private FPlayer _requestedBy;

    private Vector3 _spawnLocation;

    public FPlayer RequestedBy => this._requestedBy;

    public Vector3 SpawnLocation => this._spawnLocation;

    FPlayer IInternalAssistanceService.RequestedBy
    {
        get => this._requestedBy;
        set => this._requestedBy = value;
    }

    Vector3 IInternalAssistanceService.SpawnLocation
    {
        get => this._spawnLocation;
        set => this._spawnLocation = value;
    }

    public abstract Task Setup();

    public abstract Task<bool> CanRun();

    public abstract Task OnCalledAsync(CancellationTokenSource cancellationTokenSource);

    public abstract Task OnCancellationRequestedAsync();

    public abstract Task OnCompletedAsync();
}

internal interface IInternalAssistanceService
{
    public FPlayer RequestedBy { get; set; }

    public Vector3 SpawnLocation { get; set; }
}
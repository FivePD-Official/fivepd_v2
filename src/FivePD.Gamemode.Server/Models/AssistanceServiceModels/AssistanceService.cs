using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivePD.Gamemode.Server.Models.AssistanceServiceModels;

#pragma warning disable

public abstract class AssistanceService : IInternalAssistanceService
{
    private Vector3 _spawnLocation;

    private Vector3 _target;

    public Vector3 SpawnLocation => this._spawnLocation;

    public Vector3 Target => this._target;

    Vector3 IInternalAssistanceService.SpawnLocation
    {
        get => this._spawnLocation;
        set => this._spawnLocation = value;
    }

    Vector3 IInternalAssistanceService.Target
    {
        get => this._target;
        set => this._target = value;
    }

    public abstract Task OnCalledAsync(CancellationToken cancellationToken);

    public abstract Task OnCancellationRequestedAsync();

    public abstract Task OnCompleteAsync();
}

internal interface IInternalAssistanceService
{
    internal Vector3 SpawnLocation { get; set; }

    internal Vector3 Target { get; set; }
}
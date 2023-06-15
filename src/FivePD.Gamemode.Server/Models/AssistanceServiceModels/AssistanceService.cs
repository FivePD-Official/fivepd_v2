using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivePD.Gamemode.Server.Models.AssistanceServiceModels;

#pragma warning disable

public abstract class AssistanceService : IInternalAssistanceService
{
    private Vector3 _location;

    public Vector3 Location => this._location;

    Vector3 IInternalAssistanceService.Location
    {
        get => this._location;
        set => this._location = value;
    }
    
    public abstract Task SpawnEntitiesAsync();

    public abstract Task GoToLocationAsync();

    public abstract Task PerformActionAsync();

    public abstract Task ActionCompletedOrCanceledAsync();
}

internal interface IInternalAssistanceService
{
    internal Vector3 Location { get; set; } 
}
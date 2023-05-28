using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Requests;
using FivePD.Gamemode.Client.Requests;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Represents the tasks of a <see cref="FPed"/>.
/// </summary>
public sealed class FPedTasks
{
    private readonly FPed _fPed;
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FPedTasks"/> class.
    /// </summary>
    /// <param name="fPed">The <see cref="FPed"/> to use.</param>
    /// <param name="eventService">The <see cref="IEventService"/> to use.</param>
    public FPedTasks(FPed fPed, IEventService eventService)
    {
        this._fPed = fPed;
        this._eventService = eventService;
    }

    /// <summary>
    /// Invokes the "ped.Task.StandStill" native.
    /// </summary>
    /// <param name="duration">The duration in milliseconds. If -1, the ped will stand still until the task is cleared.</param>
    public void StandStill(int duration) => this.SetTask(SupportedFPedTasks.StandStill, new PedTasks.StandStill { Duration = duration });

    /// <summary>
    /// Invokes the "ped.Task.TurnTo" native.
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> to turn to.</param>
    /// <param name="duration">The duration in milliseconds. If -1, the ped will turn until the task is cleared.</param>
    public void TurnTo(Entity entity, int duration = -1) => this.SetTask(
        SupportedFPedTasks.TurnToEntity,
        new PedTasks.TurnToEntity { Duration = duration, EntityNetworkId = entity.NetworkId });

    /// <summary>
    /// Invokes the "ped.Task.TurnTo" native.
    /// </summary>
    /// <param name="position">The <see cref="Vector3"/> to turn to.</param>
    /// <param name="duration">The duration in milliseconds. If -1, the ped will turn until the task is cleared.</param>
    public void TurnTo(Vector3 position, int duration = -1) => this.SetTask(
        SupportedFPedTasks.TurnToPosition,
        new PedTasks.TurnToPosition { Duration = duration, Position = position });

    private void SetTask<T>(string task, T args)
    {
        this._eventService.TriggerEventWithoutCallback(
            this._fPed.Owner,
            Events.Ped.SetTask,
            JsonConvert.SerializeObject(new SetPedTaskRequest
            {
                NetworkId = this._fPed.GetFPed.NetworkId,
                Task = task,
                Args = JsonConvert.SerializeObject(args),
            }));
    }
}
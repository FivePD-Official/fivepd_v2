using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Common.Requests;
using FivePD.Common.Requests.PedTasks;
using FivePD.Gamemode.Common.Utilities;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.EntityWrappers;

/// <summary>
/// Represents a wrapper around a <see cref="CommonFPed"/> to provide additional functionality.
/// </summary>
public sealed class FPed : FEntity
{
    private readonly CommonFPed _fPed;

    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPed">The FPed to use.</param>
    /// <param name="eventService">The <see cref="IEventService"/> to use.</param>
    public FPed(CommonFPed fPed, IEventService eventService)
    {
        this._fPed = fPed;
        this._eventService = eventService;
        this.Properties = new(this);
        this.Task = new(this);
    }

    /// <summary>
    /// Gets the properties of the <see cref="FPed"/>.
    /// </summary>
    public FPedProperties Properties { get; private set; }

    /// <summary>
    /// Gets the tasks of the <see cref="FPed"/>.
    /// </summary>
    public FPedTasks Task { get; private set; }

    /// <summary>
    /// Gets the underlying <see cref="CommonFPed"/>.
    /// </summary>
    public CommonFPed GetFPed => this._fPed;

    /// <summary>
    /// Gets the underlying <see cref="Ped"/>.
    /// </summary>
    public Ped GetUnderlyingPed => (Ped)Entity.FromNetworkId(this._fPed.NetworkId);

    /// <summary>
    /// Gets the owner of the <see cref="FPed"/>.
    /// </summary>
    public Player Owner => this.GetUnderlyingPed.Owner;

    /// <summary>
    /// Represents the properties of a <see cref="FPed"/>.
    /// </summary>
    public sealed class FPedProperties
    {
        private readonly FPed _fPed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FPedProperties"/> class.
        /// </summary>
        /// <param name="fPed">The <see cref="FPed"/> to use.</param>
        public FPedProperties(FPed fPed)
        {
            this._fPed = fPed;
        }

        /// <summary>
        /// Gets the gender of the <see cref="FPed"/>.
        /// </summary>
        public FGender Gender => this._fPed.GetFPed.Gender;

        /// <summary>
        /// Gets whether the ped is dead or not.
        /// </summary>
        public Task<bool> IsDead => this.GetPropertyBoolAsync(SupportedFPedProperties.IsDead);

        /// <summary>
        /// Gets whether the ped is on foot or not.
        /// </summary>
        public Task<bool> IsOnFoot => this.GetPropertyBoolAsync(SupportedFPedProperties.IsOnFoot);

        /// <summary>
        /// Gets whether the ped is in any vehicle or not.
        /// </summary>
        public Task<bool> IsInVehicle => this.GetPropertyBoolAsync(SupportedFPedProperties.IsInVehicle);

        /// <summary>
        /// Sets the "BlockPermanentEvents" property of the ped.
        /// </summary>
        /// <param name="value">true to block permanent events; otherwise, false.</param>
        public void BlockPermanentEvents(bool value) => this.SetPropertyBool(SupportedFPedProperties.BlockPermanentEvents, value);

        /// <summary>
        /// Sets the "AlwaysKeepTask" property of the ped.
        /// </summary>
        /// <param name="value">true to block permanent events; otherwise, false.</param>
        public void AlwaysKeepTask(bool value) => this.SetPropertyBool(SupportedFPedProperties.AlwaysKeepTask, value);

        /// <summary>
        /// Creates a blip for the <see cref="FPed"/>.
        /// </summary>
        /// <param name="sprite">The <see cref="FBlipSprite"/> to use.</param>
        /// <param name="color">The <see cref="FBlipColor"/> to use.</param>
        /// <param name="text">The text to display on the blip. If <see langword="null"/>, the blip will not have any text.</param>
        public void CreateBlip(FBlipSprite sprite, FBlipColor color, string text)
        {
            this._fPed._eventService.TriggerEventWithoutCallback(
                this._fPed.Owner,
                Events.Ped.CreateBlip,
                JsonConvert.SerializeObject(new CreateBlipRequest
                {
                    NetworkId = this._fPed.GetFPed.NetworkId,
                    Sprite = sprite,
                    Color = color,
                    Text = text,
                }));
        }

        /// <summary>
        /// Removes all blips from the <see cref="FPed"/>.
        /// </summary>
        public void RemoveBlips()
        {
            this._fPed._eventService.TriggerEventWithoutCallback(
                this._fPed.Owner,
                Events.Ped.RemoveBlips,
                JsonConvert.SerializeObject(new RemoveBlipsRequest
                {
                    NetworkId = this._fPed.GetFPed.NetworkId,
                }));
        }

        private async Task<bool> GetPropertyBoolAsync(string property)
        {
            var value = await this._fPed._eventService.TriggerEventAsync(this._fPed.Owner, Events.Ped.GetProperty, this._fPed.GetFPed.NetworkId, property);
            return bool.Parse(value);
        }

        private void SetPropertyBool(string property, bool value)
        {
            this._fPed._eventService.TriggerEventAsync(this._fPed.Owner, Events.Ped.SetProperty, this._fPed.GetFPed.NetworkId, property, value.ToString());
        }
    }

    /// <summary>
    /// Represents the tasks of a <see cref="FPed"/>.
    /// </summary>
    public sealed class FPedTasks
    {
        private readonly FPed _fPed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FPedTasks"/> class.
        /// </summary>
        /// <param name="fPed">The <see cref="FPed"/> to use.</param>
        public FPedTasks(FPed fPed)
        {
            this._fPed = fPed;
        }

        /// <summary>
        /// Invokes the "ped.Task.StandStill" native.
        /// </summary>
        /// <param name="duration">The duration in milliseconds. If -1, the ped will stand still until the task is cleared.</param>
        public void StandStill(int duration) => this.SetTask(SupportedFPedTasks.StandStill, new StandStill { Duration = duration });

        /// <summary>
        /// Invokes the "ped.Task.TurnTo" native.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to turn to.</param>
        /// <param name="duration">The duration in milliseconds. If -1, the ped will turn until the task is cleared.</param>
        public void TurnTo(Entity entity, int duration = -1) => this.SetTask(
            SupportedFPedTasks.TurnToEntity,
            new TurnToEntity { Duration = duration, EntityNetworkId = entity.NetworkId });

        /// <summary>
        /// Invokes the "ped.Task.TurnTo" native.
        /// </summary>
        /// <param name="position">The <see cref="Vector3"/> to turn to.</param>
        /// <param name="duration">The duration in milliseconds. If -1, the ped will turn until the task is cleared.</param>
        public void TurnTo(Vector3 position, int duration = -1) => this.SetTask(
            SupportedFPedTasks.TurnToPosition,
            new TurnToPosition { Duration = duration, Position = position });

        private void SetTask<T>(string task, T args)
        {
            this._fPed._eventService.TriggerEventWithoutCallback(
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
}
using System.Threading.Tasks;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Gamemode.Client.Requests;
using FivePD.Gamemode.Common.Utilities;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.EntityWrappers.PedWrapper;

/// <summary>
/// Represents the properties of a <see cref="FPed"/>.
/// </summary>
public sealed class FPedProperties
{
    private readonly FPed _fPed;
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FPedProperties"/> class.
    /// </summary>
    /// <param name="fPed">The <see cref="FPed"/> to use.</param>
    public FPedProperties(FPed fPed)
    {
        this._fPed = fPed;
        this._eventService = ServiceLocator.GetService<IEventService>();
    }

    /// <summary>
    /// Gets the gender of the <see cref="FPed"/>.
    /// </summary>
    public FGender Gender => this._fPed.GetFPed.Gender;

    /// <summary>
    /// Gets whether the ped is dead or not.
    /// </summary>
    public Task<bool> IsDead => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsDead);

    /// <summary>
    /// Gets whether the ped is on foot or not.
    /// </summary>
    public Task<bool> IsOnFoot => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsOnFoot);

    /// <summary>
    /// Gets whether the ped is in any vehicle or not.
    /// </summary>
    public Task<bool> IsInVehicle => this.GetPropertyBoolAsync(FPedPropertiesDTO.Events.IsInVehicle);

    /// <summary>
    /// Sets the "BlockPermanentEvents" property of the ped.
    /// </summary>
    /// <param name="value">true to block permanent events; otherwise, false.</param>
    public void BlockPermanentEvents(bool value) => this.SetPropertyBool(FPedPropertiesDTO.Events.BlockPermanentEvents, value);

    /// <summary>
    /// Sets the "AlwaysKeepTask" property of the ped.
    /// </summary>
    /// <param name="value">true to block permanent events; otherwise, false.</param>
    public void AlwaysKeepTask(bool value) => this.SetPropertyBool(FPedPropertiesDTO.Events.AlwaysKeepTask, value);

    /// <summary>
    /// Creates a blip for the <see cref="FPed"/>.
    /// </summary>
    /// <param name="sprite">The <see cref="FBlipSprite"/> to use.</param>
    /// <param name="color">The <see cref="FBlipColor"/> to use.</param>
    /// <param name="text">The text to display on the blip. If <see langword="null"/>, the blip will not have any text.</param>
    public void CreateBlip(FBlipSprite sprite, FBlipColor color, string text)
    {
        this.SetPropertyString(FPedPropertiesDTO.Events.CreateBlip, JsonConvert.SerializeObject(new FPedPropertiesDTO.CreateBlipRequest
        {
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
        this.SetPropertyString(FPedPropertiesDTO.Events.RemoveBlips, string.Empty);
    }

    private async Task<bool> GetPropertyBoolAsync(string property)
    {
        var value = await this._eventService.TriggerEventAsync(this._fPed.Owner, Events.Ped.GetProperty, this._fPed.GetFPed.NetworkId, property);
        return bool.Parse(value);
    }

    private void SetPropertyString(string property, string value)
    {
        this._eventService.TriggerEventWithoutCallback(this._fPed.Owner, Events.Ped.SetProperty, this._fPed.GetFPed.NetworkId, property, value);
    }

    private void SetPropertyBool(string property, bool value)
    {
        this._eventService.TriggerEventWithoutCallback(this._fPed.Owner, Events.Ped.SetProperty, this._fPed.GetFPed.NetworkId, property, value.ToString());
    }
}
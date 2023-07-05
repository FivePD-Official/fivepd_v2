using System.Threading.Tasks;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.EntityWrappers.VehicleWrapper;

/// <summary>
/// Represents the properties of a <see cref="FPed"/>.
/// </summary>
public sealed class FVehicleProperties
{
    private readonly FVehicle _fVehicle;
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FVehicleProperties"/> class.
    /// </summary>
    /// <param name="fVehicle">The <see cref="FVehicle"/> to use.</param>
    public FVehicleProperties(FVehicle fVehicle)
    {
        this._fVehicle = fVehicle;
        this._eventService = ServiceLocator.GetService<IEventService>();
    }

    /// <summary>
    /// Sets the "BlockPermanentEvents" property of the ped.
    /// </summary>
    /// <param name="value">true to block permanent events; otherwise, false.</param>
    public void IsSirenActive(bool value) => this.SetPropertyBool(FVehiclePropertiesDTO.Events.IsSirenActive, value);

    /// <summary>
    /// Invokes the "MarkAsNoLongerNeeded" method on the ped.
    /// </summary>
    public void MarkAsNoLongerNeeded() => this.InvokeMethod<object>(FVehiclePropertiesDTO.Events.MarkAsNoLongerNeeded);

    /// <summary>
    /// Creates a blip for the <see cref="FPed"/>.
    /// </summary>
    /// <param name="sprite">The <see cref="FBlipSprite"/> to use.</param>
    /// <param name="color">The <see cref="FBlipColor"/> to use.</param>
    /// <param name="text">The text to display on the blip. If <see langword="null"/>, the blip will not have any text.</param>
    public void CreateBlip(FBlipSprite sprite, FBlipColor color, string text)
    {
        this.SetPropertyString(FVehiclePropertiesDTO.Events.CreateBlip, JsonConvert.SerializeObject(new FVehiclePropertiesDTO.CreateBlipRequest
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
        this.SetPropertyString(FVehiclePropertiesDTO.Events.RemoveBlips, string.Empty);
    }

    private async Task<bool> GetPropertyBoolAsync(string property)
    {
        var value = await this._eventService.TriggerEventAsync(this._fVehicle.Owner, Events.Vehicle.GetProperty, this._fVehicle.GetFVehicle.NetworkId, property);
        return bool.Parse(value);
    }

    private void SetPropertyString(string property, string value)
    {
        this._eventService.TriggerEventWithoutCallback(this._fVehicle.Owner, Events.Vehicle.SetProperty, this._fVehicle.GetFVehicle.NetworkId, property, value);
    }

    private void SetPropertyBool(string property, bool value)
    {
        this._eventService.TriggerEventWithoutCallback(this._fVehicle.Owner, Events.Vehicle.SetProperty, this._fVehicle.GetFVehicle.NetworkId, property, value.ToString());
    }

    private void InvokeMethod<T>(string method, T args = default)
    {
        this._eventService.TriggerEventWithoutCallback(
            this._fVehicle.Owner,
            Events.Vehicle.InvokeMethod,
            JsonConvert.SerializeObject(new FVehiclePropertiesDTO.InvokeMethodRequest
            {
                NetworkId = this._fVehicle.GetFVehicle.NetworkId,
                Method = method,
                Args = JsonConvert.SerializeObject(args),
            }));
    }
}
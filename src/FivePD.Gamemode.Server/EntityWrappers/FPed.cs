using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Common.Requests;
using FivePD.Gamemode.Common.Utilities;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.EntityWrappers;

#pragma warning disable

public sealed class FPed : FEntity
{
    public FPedProperties Properties { get; private set; }

    private readonly CommonFPed _fPed;
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FPed"/> class.
    /// </summary>
    /// <param name="fPed">The FPed to use.</param>
    public FPed(CommonFPed fPed, IEventService eventService)
    {
        this._fPed = fPed;
        this._eventService = eventService;
        this.Properties = new(this);
    }

    public CommonFPed GetFPed => this._fPed;

    public Ped GetUnderlyingPed => (Ped)Entity.FromNetworkId(this._fPed.NetworkId);

    public Player Owner => this.GetUnderlyingPed.Owner;

    public sealed class FPedProperties
    {
        private readonly FPed _fPed;

        public FPedProperties(FPed fPed)
        {
            this._fPed = fPed;
        }

        public void CreateBlip(FBlipSprite sprite, FBlipColor color, string text)
        {
            this._fPed._eventService.TriggerEventWithoutCallback(
                this._fPed.Owner,
                Events.Ped.CreateBlip,
                JsonConvert.SerializeObject(new CreateBlipRequest
                {
                    NetworkId = this._fPed._fPed.NetworkId,
                    Sprite = sprite,
                    Color = color,
                    Text = text,
                }));
        }

        public void RemoveBlips()
        {
            this._fPed._eventService.TriggerEventWithoutCallback(
                this._fPed.Owner,
                Events.Ped.RemoveBlips,
                JsonConvert.SerializeObject(new RemoveBlipsRequest
                {
                    NetworkId = this._fPed._fPed.NetworkId,
                }));
        }

        public FGender Gender => this._fPed._fPed.Gender;

        public Task<bool> IsDead => this.GetPropertyBool(SupportedFPedProperties.IsDead);

        public Task<bool> IsOnFoot => this.GetPropertyBool(SupportedFPedProperties.IsOnFoot);

        public Task<bool> IsInVehicle => this.GetPropertyBool(SupportedFPedProperties.IsInVehicle);

        public void BlockPermanentEvents(bool value) => this.SetPropertyBool(SupportedFPedProperties.BlockPermanentEvents, value);

        private async Task<bool> GetPropertyBool(string property)
        {
            var value = await this._fPed._eventService.TriggerEventAsync(this._fPed.Owner, Events.Ped.GetProperty, this._fPed._fPed.NetworkId, property);
            return bool.Parse(value);
        }

        private void SetPropertyBool(string property, bool value)
        {
            this._fPed._eventService.TriggerEventAsync(this._fPed.Owner, Events.Ped.SetProperty, this._fPed._fPed.NetworkId, property, value.ToString());
        }
    }
}
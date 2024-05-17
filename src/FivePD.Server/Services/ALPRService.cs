using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IAlprService" />
public class AlprService : IAlprService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlprService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">Event handler dictionary.</param>
    public AlprService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary[Events.ALPR.Check] += Check;
    }

    private (bool Valid, string Label) GetDocumentStatus(FVehicle fVehicle, VehicleDocumentType vehicleDocumentType)
    {
        var text = new GtaStyleText();

        var hasDocument = fVehicle.HasDocument(vehicleDocumentType);
        if (!hasDocument)
        {
            return (false, text.Red("None").Build());
        }

        var document = fVehicle.GetDocument(vehicleDocumentType);
        if (document.Status == VehicleDocumentStatus.Valid)
        {
            return (true, text.Green("Valid").Build());
        }

        return (false,
            text.Red(document.Status switch
            {
                VehicleDocumentStatus.Expired => "Expired",
                VehicleDocumentStatus.Suspended => "Suspended",
                VehicleDocumentStatus.Revoked => "Revoked",
                _ => "Unknown",
            }).Build());
    }

    private void Check([FromSource] Player source, int netId)
    {
        var fVehicle = FVehicle.FromNetId(netId);
        if (fVehicle is null)
        {
            return;
        }

        var ownerPed = FPed.FromNetId(fVehicle.OwnerNetworkId);
        if (ownerPed is null)
        {
            return;
        }

        var insuranceStatus = this.GetDocumentStatus(fVehicle, VehicleDocumentType.Insurance);
        var registrationStatus = this.GetDocumentStatus(fVehicle, VehicleDocumentType.Registration);

        if (ownerPed.Warrants.Count <= 0 && insuranceStatus.Valid && registrationStatus.Valid)
        {
            return;
        }

        var content = new GtaStyleText()
            .Blue($"[ALPR]")
            .NewLine()
            .Yellow("Vehicle brand: ").White(fVehicle.Brand)
            .NewLine()
            .Yellow("Vehicle color: ").White(fVehicle.Color)
            .NewLine()
            .Yellow("License plate: ").White(fVehicle.LicensePlate)
            .NewLine()
            .Yellow("Registered owner: ").White($"{ownerPed.Firstname} {ownerPed.Lastname}")
            .NewLine()
            .Yellow("Warrant: ").White(ownerPed.Warrants.Count > 1 ? "1 or more" : ownerPed.Warrants[0])
            .NewLine()
            .Yellow($"Insurance: {insuranceStatus.Label}")
            .NewLine()
            .Yellow($"Registration: {registrationStatus.Label}")
            .Build();

        var dTO = JsonConvert.SerializeObject(new CheckNotificationDTO
        {
            WantedStatus = ownerPed.Warrants.Count > 0,
            NotificationContent = content,
        });

        BaseScript.TriggerClientEvent(source, Events.ALPR.Notify, dTO);
    }
}
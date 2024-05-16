using System;
using System.Linq;
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

/// <inheritdoc />
public class AlprService : IAlprService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IPedService _pedService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlprService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">Event handler dictionary.</param>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    public AlprService(EventHandlerDictionary eventHandlerDictionary, IPedService pedService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._pedService = pedService;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary[Events.ALPR.Check] += Check;
    }

    private string GetDocumentStatusLabel(FVehicle fVehicle, VehicleDocumentType vehicleDocumentType)
    {
        var text = new GtaStyleText();

        var hasDocument = fVehicle.HasDocument(vehicleDocumentType);
        if (!hasDocument)
        {
            return text.Red("None").Build();
        }

        var document = fVehicle.GetDocument(vehicleDocumentType);
        if (document.Status == VehicleDocumentStatus.Valid)
        {
            text.Green("Valid");
        }
        else
        {
            text.Red(document.Status switch
            {
                VehicleDocumentStatus.Expired => "Expired",
                VehicleDocumentStatus.Suspended => "Suspended",
                VehicleDocumentStatus.Revoked => "Revoked",
                _ => "Unknown",
            });
        }

        return text.Build();
    }

    private void Check([FromSource] Player source, int netId)
    {
        var emptyDTO = new AlprDTO.AlprDTO()
        {
            WantedStatus = false,
            Data = "An error occurred.",
        };

        var fVehicle = FVehicle.FromNetId(netId);
        if (fVehicle is null)
        {
            BaseScript.TriggerClientEvent(source, Events.ALPR.Check, emptyDTO);
            return;
        }

        var ownerPed = FPed.FromNetId(fVehicle.OwnerNetworkId);
        if (ownerPed is null)
        {
            BaseScript.TriggerClientEvent(source, Events.ALPR.Check, emptyDTO);
            return;
        }

        if (ownerPed.Warrants.Count > 0)
        {
            string flaggedWarrant = ownerPed.Warrants[0];

            var content = new GtaStyleText()
                .Blue($"[ALPR]")
                .NewLine()
                .Yellow($"Vehicle brand: ").White(fVehicle.Brand)
                .NewLine()
                .Yellow($"Vehicle color: ").White(fVehicle.Color)
                .NewLine()
                .Yellow($"License plate: ").White(fVehicle.LicensePlate)
                .NewLine()
                .Yellow($"Registered Owner: ").White($"{ownerPed.Firstname} {ownerPed.Lastname}")
                .NewLine()
                .Yellow($"Flagged Warrant: ").White(flaggedWarrant)
                .NewLine()
                .Yellow($"Insurance: {this.GetDocumentStatusLabel(fVehicle, VehicleDocumentType.Insurance)}")
                .NewLine()
                .Yellow($"Registration: {this.GetDocumentStatusLabel(fVehicle, VehicleDocumentType.Registration)}")
                .Build();

            var dTO = JsonConvert.SerializeObject(new AlprDTO.AlprDTO()
            {
                WantedStatus = ownerPed.Warrants.Count > 0,
                Data = content,
            });

            BaseScript.TriggerClientEvent(source, Events.ALPR.Notify, dTO);
            return;
        }

        BaseScript.TriggerClientEvent(source, Events.ALPR.Notify, emptyDTO);
    }
}
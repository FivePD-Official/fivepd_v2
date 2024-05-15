using System;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;

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
        this._eventHandlerDictionary[Events.Vehicle.FetchData] += OnFetchData;
    }

    private string GetValidity(FVehicle fVehicle, VehicleDocumentType vehicleDocumentType)
    {
        bool hasDocument = fVehicle.HasDocument(vehicleDocumentType);
        string documentString = !hasDocument ? "~r~None" : "Has";
        if (hasDocument)
        {
            documentString = fVehicle.GetDocument(vehicleDocumentType).Status.ToString() == VehicleDocumentStatus.Valid.ToString()
                ? $"~g~Valid"
                : $"~r~{fVehicle.GetDocument(vehicleDocumentType).Status.ToString()}";
        }

        return documentString;
    }

    private void OnFetchData([FromSource] Player source, int netId)
    {
        var fVehicle = FVehicle.FromNetId(netId);
        var ownerPed = FPed.FromNetId(fVehicle.OwnerNetworkId);
        if (ownerPed.Warrants.Count > 0)
        {
            string flaggedWarrant = ownerPed.Warrants[0];
            string insurance = this.GetValidity(fVehicle, VehicleDocumentType.Insurance);
            string registration = this.GetValidity(fVehicle, VehicleDocumentType.Registration);

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
                .Yellow($"Insurance: ").White(insurance)
                .NewLine()
                .Yellow($"Registration: ").White(registration)
                .Build();

            BaseScript.TriggerClientEvent(source, Events.Vehicle.FetchData, ownerPed.Warrants.Count > 0, content);
            return;
        }

        BaseScript.TriggerClientEvent(source, Events.Vehicle.FetchData, ownerPed.Warrants.Count > 0);
    }
}
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IAlprService" />
public class AlprService : IAlprService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IVehicleService _vehicleService;
    private readonly IPedService _pedService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlprService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="vehicleService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="pedService">The <see cref="IPedService"/> to use.</param>
    public AlprService(
        EventHandlerDictionary eventHandlerDictionary,
        IVehicleService vehicleService,
        IPedService pedService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._vehicleService = vehicleService;
        this._pedService = pedService;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.ALPR.Check] += Check;
    }

    private (bool Valid, string Label) GetVehicleDocumentStatus(FVehicle fVehicle, VehicleDocumentType vehicleDocumentType)
    {
        var hasDocument = fVehicle.HasDocument(vehicleDocumentType);
        if (!hasDocument)
        {
            return (false, "LABEL.NONE");
        }

        var document = fVehicle.GetDocument(vehicleDocumentType);
        if (document.Status == VehicleDocumentStatus.Valid)
        {
            return (true, "LABEL.VALID");
        }

        return (false,
            document.Status switch
            {
                VehicleDocumentStatus.Expired => "LABEL.EXPIRED",
                VehicleDocumentStatus.Suspended => "LABEL.SUSPENDED",
                VehicleDocumentStatus.Revoked => "LABEL.REVOKED",
                _ => "LABEL.UNKNOWN",
            });
    }

    private void Check([FromSource] Player source, int netId)
    {
        var fVehicle = this._vehicleService.TryGetVehicle(netId);
        if (fVehicle is null)
        {
            return;
        }

        var ownerPed = this._pedService.TryGetPed(fVehicle.OwnerNetworkId);
        if (ownerPed is null)
        {
            return;
        }

        var insuranceStatus = this.GetVehicleDocumentStatus(fVehicle, VehicleDocumentType.Insurance);
        var registrationStatus = this.GetVehicleDocumentStatus(fVehicle, VehicleDocumentType.Registration);
        var pedDrivingLicense = ownerPed.GetLicense(PedLicenseType.Driving);

        var a = ownerPed.Warrants.Count <= 0 && insuranceStatus.Valid && registrationStatus.Valid && pedDrivingLicense is not null && pedDrivingLicense.Status == PedLicenseStatus.Valid;
        Debug.WriteLine($"a: {a} -- {insuranceStatus.Valid} - {registrationStatus.Valid} - {pedDrivingLicense?.Status}");

        if (a)
        {
            return;
        }

        var pedDrivingLicenseStatusLabel = pedDrivingLicense?.Status switch
        {
            PedLicenseStatus.Expired => "Expired",
            PedLicenseStatus.Suspended => "Suspended",
            PedLicenseStatus.Revoked => "Revoked",
            _ => "Unknown",
        };

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
            .Yellow($"Insurance: {insuranceStatus.Label}")
            .NewLine()
            .Yellow($"Registration: {registrationStatus.Label}")
            .NewLine()
            .Yellow($"Owner driving license: {pedDrivingLicenseStatusLabel}")
            .Build();

        var dTO = JsonConvert.SerializeObject(new CheckNotificationDTO
        {
            WantedStatus = ownerPed.Warrants.Count > 0,
            NotificationContent = content,
        });

        BaseScript.TriggerClientEvent(source, Events.ALPR.Notify, dTO);
    }
}
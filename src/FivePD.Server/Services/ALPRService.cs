using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Utilities;
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
    private readonly IVehicleStorageService _vehicleStorageService;
    private readonly IPedStorageService _pedStorageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlprService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="vehicleStorageService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="pedStorageService">The <see cref="IPedService"/> to use.</param>
    public AlprService(
        EventHandlerDictionary eventHandlerDictionary,
        IVehicleStorageService vehicleStorageService,
        IPedStorageService pedStorageService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._vehicleStorageService = vehicleStorageService;
        this._pedStorageService = pedStorageService;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.ALPR.Check] += this.CheckVehicle;
    }

    private (bool Valid, string Label) GetVehicleDocumentStatus(FVehicle fVehicle, VehicleDocumentType vehicleDocumentType)
    {
        var vehicleHistory = this._vehicleStorageService.GetVehicleHistory(this._vehicleStorageService.TryGetVehicle(fVehicle.NetworkId));
        if (vehicleHistory is null)
        {
            return (false, "LABEL.UNKNOWN");
        }

        var hasDocument = vehicleHistory.HasDocument(vehicleDocumentType);
        if (!hasDocument)
        {
            return (false, "LABEL.NONE");
        }

        var document = vehicleHistory.GetDocument(vehicleDocumentType);
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

    private void CheckVehicle([FromSource] Player source, int vehicleNetworkId)
    {
        var fVehicle = this._vehicleStorageService.TryGetVehicle(vehicleNetworkId);
        if (fVehicle is null)
        {
            return;
        }

        var vehicleHistory = this._vehicleStorageService.GetVehicleHistory(fVehicle);
        if (vehicleHistory is null)
        {
            return;
        }

        var ownerPed = this._pedStorageService.GetPedHistory(this._pedStorageService.TryGetPed(fVehicle.OwnerNetworkId));
        if (ownerPed is null)
        {
            return;
        }

        var insuranceStatus = this.GetVehicleDocumentStatus(fVehicle, VehicleDocumentType.Insurance);
        var registrationStatus = this.GetVehicleDocumentStatus(fVehicle, VehicleDocumentType.Registration);
        var pedDrivingLicense = ownerPed.GetLicense(PedLicenseType.Driving);

        var a = ownerPed.Warrants.Count <= 0 && insuranceStatus.Valid && registrationStatus.Valid && pedDrivingLicense is not null &&
                pedDrivingLicense.Status == PedLicenseStatus.Valid;
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
            .Yellow("Vehicle brand: ").White(vehicleHistory.Brand)
            .NewLine()
            .Yellow("Vehicle color: ").White(vehicleHistory.Color)
            .NewLine()
            .Yellow("License plate: ").White(vehicleHistory.LicensePlate)
            .NewLine()
            .Yellow("Registered owner: ").White($"{ownerPed.Firstname} {ownerPed.Lastname}")
            .NewLine()
            .Yellow($"Insurance: {insuranceStatus.Label}")
            .NewLine()
            .Yellow($"Registration: {registrationStatus.Label}")
            .NewLine()
            .Yellow($"Owner driving license: {pedDrivingLicenseStatusLabel}")
            .Build();

        BaseScript.TriggerClientEvent(
            source,
            Events.ALPR.Notify,
            JsonConvert.SerializeObject(new CheckNotificationDTO
            {
                WantedStatus = ownerPed.Warrants.Count > 0,
                NotificationContent = content,
            }));
    }
}
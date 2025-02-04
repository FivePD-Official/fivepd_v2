using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Server.Interfaces.IAlprService" />
public class AlprService : IAlprService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IVehicleStorageService _vehicleStorageService;
    private readonly IPedStorageService _pedStorageService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IConfigService _configService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlprService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="vehicleStorageService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="pedStorageService">The <see cref="IPedService"/> to use.</param>
    /// <param name="gameNotificationService">The <see cref="IGameNotificationService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    public AlprService(
        EventHandlerDictionary eventHandlerDictionary,
        IVehicleStorageService vehicleStorageService,
        IPedStorageService pedStorageService,
        IGameNotificationService gameNotificationService,
        IConfigService configService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._vehicleStorageService = vehicleStorageService;
        this._pedStorageService = pedStorageService;
        this._gameNotificationService = gameNotificationService;
        this._configService = configService;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.ALPR.Check] += this.CheckVehicle;
    }

    private (bool Valid, string Label) GetVehicleDocumentStatus(FVehicle fVehicle, VehicleDocumentType vehicleDocumentType)
    {
        var vehicleData = this._vehicleStorageService.GetVehicleData(this._vehicleStorageService.TryGetVehicle(fVehicle.NetworkId));
        if (vehicleData is null)
        {
            return (false, "LABEL.UNKNOWN");
        }

        var hasDocument = vehicleData.HasDocument(vehicleDocumentType);
        if (!hasDocument)
        {
            return (false, "LABEL.NONE");
        }

        var document = vehicleData.GetDocument(vehicleDocumentType);
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
        // TODO: filter police vehicles from the db config
        var fVehicle = this._vehicleStorageService.TryGetVehicle(vehicleNetworkId);
        if (fVehicle is null)
        {
            return;
        }

        var vehicleData = this._vehicleStorageService.GetVehicleData(fVehicle);
        if (vehicleData is null)
        {
            return;
        }

        var ownerPed = this._pedStorageService.GetPedData(this._pedStorageService.TryGetPed(fVehicle.OwnerNetworkId));
        if (ownerPed is null)
        {
            return;
        }

        var insuranceStatus = this.GetVehicleDocumentStatus(fVehicle, VehicleDocumentType.Insurance);
        var registrationStatus = this.GetVehicleDocumentStatus(fVehicle, VehicleDocumentType.Registration);
        var pedDrivingLicense = ownerPed.GetLicense(PedLicenseType.Driving);

        var a = ownerPed.Warrants.Count <= 0 && insuranceStatus.Valid && registrationStatus.Valid && pedDrivingLicense is not null &&
                pedDrivingLicense.Status == PedLicenseStatus.Valid;
        Debug.WriteLine($"a: {a} -- {vehicleData.LicensePlate} -- {insuranceStatus.Valid} - {registrationStatus.Valid} - {pedDrivingLicense?.Status}");

        if (a)
        {
            return;
        }

        var pedDrivingLicenseStatusLabel = pedDrivingLicense?.Status switch
        {
            PedLicenseStatus.Expired => "LABEL.EXPIRED",
            PedLicenseStatus.Suspended => "LABEL.SUSPENDED",
            PedLicenseStatus.Revoked => "LABEL.REVOKED",
            _ => "LABEL.UNKNOWN",
        };

        // TODO: fix translation both in dotnet and in js
        return;

        var message = new StringBuilder()
            .Append("[ALPR]")
            .Append("{{NEW_LINE}}")
            .Append("ALPR.NOTIFICATION.CONTENT.VEHICLE_BRAND")
            .Append("{{NEW_LINE}}")
            .Append("ALPR.NOTIFICATION.CONTENT.VEHICLE_COLOR")
            .Append("{{NEW_LINE}}")
            .Append("ALPR.NOTIFICATION.CONTENT.LICENSE_PLATE")
            .Append("{{NEW_LINE}}")
            .Append("ALPR.NOTIFICATION.CONTENT.REGISTERED_OWNER")
            .Append("{{NEW_LINE}}")
            .Append("ALPR.NOTIFICATION.CONTENT.INSURANCE")
            .Append("{{NEW_LINE}}")
            .Append("ALPR.NOTIFICATION.CONTENT.REGISTRATION")
            .Append("{{NEW_LINE}}")
            .Append("ALPR.NOTIFICATION.CONTENT.OWNER_DRIVING_LICENSE")
            .ToString();

        var asd = $"[ALPR]{{NEW_LINE}}{{BRAND(brand)}}";

        Debug.WriteLine($"NOTIFY ALPR - {vehicleData.LicensePlate}");
        this._gameNotificationService.Notify(source, new GameNotificationOptions
        {
            Title = "[ALPR]",
            Message = asd,
            MessageParams = new Dictionary<string, string>
            {
                ["BRAND"] = "ALPR.NOTIFICATION.CONTENT.VEHICLE_BRAND",
                ["brand"] = vehicleData.Brand,
                ["color"] = vehicleData.Color,
                ["license_plate"] = vehicleData.LicensePlate,
                ["registered_owner"] = $"{ownerPed.Firstname} {ownerPed.Lastname}",
                ["insurance"] = insuranceStatus.Label,
                ["registration"] = registrationStatus.Label,
                ["owner_driving_license"] = pedDrivingLicenseStatusLabel,
            },
            PlayAudio = this._configService.GeneralConfig.Audio.Files.AlprToggle,
        });
    }
}
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Server.Interfaces.IAlprService" />
[Obfuscate]
public class AlprService : IAlprService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IVehicleStorageService _vehicleStorageService;
    private readonly IPedStorageService _pedStorageService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IConfigService _configService;
    private readonly DatabaseContext _databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlprService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="vehicleStorageService">The <see cref="IVehicleService"/> to use.</param>
    /// <param name="pedStorageService">The <see cref="IPedService"/> to use.</param>
    /// <param name="gameNotificationService">The <see cref="IGameNotificationService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    public AlprService(
        EventHandlerDictionary eventHandlerDictionary,
        IVehicleStorageService vehicleStorageService,
        IPedStorageService pedStorageService,
        IGameNotificationService gameNotificationService,
        IConfigService configService,
        DatabaseContext databaseContext)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._vehicleStorageService = vehicleStorageService;
        this._pedStorageService = pedStorageService;
        this._gameNotificationService = gameNotificationService;
        this._configService = configService;
        this._databaseContext = databaseContext;
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
                VehicleDocumentStatus.Valid => "LABEL.VALID",
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

        this._databaseContext.Entry(ownerPed)
            .Collection(x => x.Licenses)
            .Load();

        var insuranceStatus = this.GetVehicleDocumentStatus(fVehicle, VehicleDocumentType.Insurance);
        var registrationStatus = this.GetVehicleDocumentStatus(fVehicle, VehicleDocumentType.Registration);
        var pedDrivingLicense = ownerPed.GetLicense(PedLicenseType.Driving);

        if (ownerPed.Warrants.Count <= 0 && insuranceStatus.Valid && registrationStatus.Valid && pedDrivingLicense is not null &&
            pedDrivingLicense.Status == PedLicenseStatus.Valid)
        {
            return;
        }

        var pedDrivingLicenseStatusLabel = pedDrivingLicense?.Status switch
        {
            PedLicenseStatus.Valid => "LABEL.VALID",
            PedLicenseStatus.Expired => "LABEL.EXPIRED",
            PedLicenseStatus.Suspended => "LABEL.SUSPENDED",
            PedLicenseStatus.Revoked => "LABEL.REVOKED",
            _ => "LABEL.UNKNOWN",
        };

        this._gameNotificationService.Notify(source, new GameNotificationOptions
        {
            Title = "[{{ALPR}}]",
            TitleParams = new Dictionary<string, string>
            {
                ["ALPR"] = "UI.ALPR.TITLE",
            },
            Message = "{{BRAND(x)}}{{NEW_LINE}}{{COLOR(x)}}{{NEW_LINE}}{{LICENSE_PLATE(x)}}{{NEW_LINE}}{{REGISTERED_OWNER(x)}}{{NEW_LINE}}{{INSURANCE(x)}}{{NEW_LINE}}{{REGISTRATION(x)}}{{NEW_LINE}}{{OWNER_DRIVING_LICENSE(x)}}",
            MessageParams = new Dictionary<string, string>
            {
                ["BRAND"] = "UI.ALPR.NOTIFICATION.CONTENT.VEHICLE_BRAND",
                ["BRAND(x)"] = vehicleData.Brand,
                ["COLOR"] = "UI.ALPR.NOTIFICATION.CONTENT.VEHICLE_COLOR",
                ["COLOR(x)"] = vehicleData.Color,
                ["LICENSE_PLATE"] = "UI.ALPR.NOTIFICATION.CONTENT.LICENSE_PLATE",
                ["LICENSE_PLATE(x)"] = vehicleData.LicensePlate,
                ["REGISTERED_OWNER"] = "UI.ALPR.NOTIFICATION.CONTENT.REGISTERED_OWNER",
                ["REGISTERED_OWNER(x)"] = $"{ownerPed.Firstname} {ownerPed.Lastname}",
                ["INSURANCE"] = "UI.ALPR.NOTIFICATION.CONTENT.INSURANCE",
                ["INSURANCE(x)"] = insuranceStatus.Label,
                ["REGISTRATION"] = "UI.ALPR.NOTIFICATION.CONTENT.REGISTRATION",
                ["REGISTRATION(x)"] = registrationStatus.Label,
                ["OWNER_DRIVING_LICENSE"] = "UI.ALPR.NOTIFICATION.CONTENT.OWNER_DRIVING_LICENSE",
                ["OWNER_DRIVING_LICENSE(x)"] = pedDrivingLicenseStatusLabel,
            },
            PlayAudio = this._configService.GeneralConfig.Audio.Files.AlprToggle,
        });
    }
}
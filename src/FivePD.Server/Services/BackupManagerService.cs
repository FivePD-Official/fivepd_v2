using System.Collections.Generic;
using System.Linq;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.Enums;
using FivePD.Common.Extensions;
using FivePD.Common.Models;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class BackupManagerService : IBackupManagerService, IOnInit
{
    private readonly IConfirmationService _confirmationService;
    private readonly IUnitService _unitService;
    private readonly IUnitManagerService _unitManagerService;
    private readonly IIncidentService _incidentService;
    private readonly IConfigService _configService;

    public BackupManagerService(
        IConfirmationService confirmationService,
        IUnitService unitService,
        IUnitManagerService unitManagerService,
        IIncidentService incidentService,
        IConfigService configService)
    {
        this._confirmationService = confirmationService;
        this._unitService = unitService;
        this._unitManagerService = unitManagerService;
        this._incidentService = incidentService;
        this._configService = configService;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._unitManagerService.OnPanicModeEnabled += (_, unit) =>
        {
            this.TriggerBackupRequest(unit, FPriority.Code99);
        };
    }

    public void TriggerBackupRequest(Unit requestingUnit, FPriority priority = FPriority.Code1)
    {
        if (!this._configService.GeneralConfig.Features.Backups)
        {
            return;
        }

        if (requestingUnit is null)
        {
            return;
        }

        IIncident incident = null;
        if (requestingUnit.Callout is not null)
        {
            incident = requestingUnit.InternalCallout;
        }
        else if (requestingUnit.TrafficStop is not null)
        {
            incident = requestingUnit.TrafficStop;
        }

        if (incident is null)
        {
            return;
        }

        var filteredUnits = this._unitService.Units()
            .Where(unit => unit.Callsign != requestingUnit.Callsign)
            .ToList();
        foreach (var unit in filteredUnits)
        {
            this._confirmationService.TriggerConfirmation(unit, new ConfirmationProperties
            {
                Title = "LABEL.DISPATCH.TITLE",
                Message = "LABEL.BACKUP.CONTENT",
                MessageParams = new Dictionary<string, string>
                {
                    ["unitCallsign"] = unit.Callsign,
                    ["priority"] = priority.GetLabelKey()
                },
                Keys = new Dictionary<string, string>
                {
                    { "F1", "UI.ACTION.ACCEPT" },
                },
                PlayAudio = this._configService.GeneralConfig.Audio.Files.BackupRequested,
            }, res =>
            {
                if (!res)
                {
                    return;
                }

                this._incidentService.AssignUnitToIncident(unit, incident);
            });
        }
    }
}
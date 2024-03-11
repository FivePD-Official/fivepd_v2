using System.Collections.Generic;
using System.Linq;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.API;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class BackupManagerService : IBackupManagerService
{
    private readonly IConfirmationService _confirmationService;
    private readonly IUnitService _unitService;
    private readonly IInternalIncidentService _incidentService;
    private readonly IConfigService _configService;
    
    public BackupManagerService(
        IConfirmationService confirmationService, 
        IUnitService unitService, 
        IIncidentService incidentService,
        IConfigService configService)
    {
        this._confirmationService = confirmationService;
        this._unitService = unitService;
        this._incidentService = (IInternalIncidentService)incidentService;
        this._configService = configService;
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
        } else if (requestingUnit.TrafficStop is not null)
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
            foreach (var p in unit.Players)
            {
                this._confirmationService.TriggerConfirmation(p, new ConfirmationProperties
                {
                    Notification = new ConfirmationProperties.NotificationProperties
                    {
                        Texture = "CHAR_CALL911",
                        Sender = "LABEL.DISPATCH",
                        Subject = "LABEL.BACKUP.TITLE",
                        Content = "LABEL.BACKUP.CONTENT", // TODO: add new params
                        ContentParams = new Dictionary<string, string>
                        {
                            ["unitCallsign"] = unit.Callsign,
                        },
                    }
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
}
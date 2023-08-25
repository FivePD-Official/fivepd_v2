using System.Collections.Generic;
using System.Linq;
using FivePD.Common.Models;
using FivePD.Server.API;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

#pragma warning disable

public class BackupManagerService : IBackupManagerService
{
    private readonly IConfirmationService _confirmationService;
    private readonly IUnitService _unitService;
    private readonly IInternalCalloutManagerService _calloutManagerService;
    private readonly IConfigService _configService;
    
    public BackupManagerService(
        IConfirmationService confirmationService, 
        IUnitService unitService, 
        ICalloutManagerService calloutManagerService,
        IConfigService configService)
    {
        this._confirmationService = confirmationService;
        this._unitService = unitService;
        this._calloutManagerService = (IInternalCalloutManagerService)calloutManagerService;
        this._configService = configService;
    }

    public void TriggerBackupRequest(Unit requestingUnit)
    {
        if (!this._configService.GeneralConfig.FeatureFlags.Backups)
        {
            return;
        }

        if (requestingUnit is null)
        {
            return;
        }

        var calloutHolder = this._calloutManagerService.ActiveCallouts.FirstOrDefault(holder =>
            holder.Callout.Units.Any(unit => unit.Callsign == requestingUnit.Callsign));
        if (calloutHolder is null)
        {
            return;
        }

        var filteredUnits = this._unitService.Units()
            .Where(unit => unit.Callsign != requestingUnit.Callsign)
            .Where(unit => this._calloutManagerService.ActiveCallouts.All(holder => holder.Callout.Units.All(u => u.Callsign != unit.Callsign)))
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
                        Content = "LABEL.BACKUP.CONTENT",
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

                    ((IInternalIncident)calloutHolder.Callout).Units.Add(unit);
                });
            }
        }
        
    }
}
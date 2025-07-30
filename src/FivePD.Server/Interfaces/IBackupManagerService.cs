using FivePD.Common.Enums;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// A service for managing unit backup requests.
/// </summary>
public interface IBackupManagerService
{
    /// <summary>
    /// Sends out a backup request to every unit.
    /// </summary>
    /// <param name="requestingUnit">The unit requesting backup.</param>
    /// <param name="priority">The priority of the request.</param>
    public void TriggerBackupRequest(Unit requestingUnit, FPriority priority = FPriority.Code1);
}
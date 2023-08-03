using FivePD.Server.API;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IBackupManagerService
{
    public void TriggerBackupRequest(Unit requestingUnit);
}
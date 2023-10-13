using FivePD.Server.API;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IIncidentService
{
    public bool AssignUnitToIncident(Unit unit, IIncident incident);
}
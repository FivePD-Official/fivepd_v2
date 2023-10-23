using System.Collections.Generic;
using FivePD.Common.DTO;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

internal record struct UnitDTO(string Callsign, bool AssignedToIncident, IEnumerable<SimplePlayerDTO> Players);
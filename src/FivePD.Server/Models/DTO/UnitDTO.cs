using System.Collections.Generic;
using FivePD.Common.DTO;
using FivePD.Server.Attributes;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal record struct UnitDTO(string Callsign, bool AssignedToIncident, IEnumerable<SimplePlayerDTO> Players);
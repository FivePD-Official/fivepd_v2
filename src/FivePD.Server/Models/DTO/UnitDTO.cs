using System.Collections.Generic;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

internal record struct UnitDTO(string Callsign, IEnumerable<SimplePlayerDTO> Players);
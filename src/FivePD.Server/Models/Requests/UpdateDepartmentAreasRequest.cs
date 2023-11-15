using System.Collections.Generic;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct UpdateDepartmentAreasRequest
{
    public List<Area> Areas { get; set; }

    public record struct Area
    {
        public int DepartmentId { get; set; }

        public List<DepartmentAreaCoordinate> Coordinates { get; set; }
    }
}
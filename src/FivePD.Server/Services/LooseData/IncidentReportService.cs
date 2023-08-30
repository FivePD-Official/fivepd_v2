using System.Collections.Generic;
using FivePD.Server.Interfaces.LooseData;
using FivePD.Server.Models;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Services.LooseData;

/// <inheritdoc />
public class IncidentReportService : LooseDataRepository<IncidentReportSet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentReportService"/> class.
    /// </summary>
    /// <param name="looseDataService">The <see cref="ILooseDataService"/> to use.</param>
    public IncidentReportService(ILooseDataService looseDataService)
        : base(looseDataService)
    {
    }

    /// <inheritdoc />
    protected override List<LooseDataAttribute> RegisterSetAttributes()
    {
        return new List<LooseDataAttribute>
        {
            new()
            {
                Name = "IncidentId",
                Label = "Incident",
                DataType = LooseDataType.Integer,
                Required = false,
                ShowInGrid = false,
                Readonly = true,
            },
            new()
            {
                Name = "UserId",
                Label = "User",
                DataType = LooseDataType.Integer,
                Required = false,
                ShowInGrid = false,
                Readonly = true,
            },
            new()
            {
                Name = "DateOfSubmit",
                Label = "Date of submit",
                DataType = LooseDataType.Date,
                Required = false,
                ShowInGrid = true,
                Readonly = true,
            },
            new()
            {
                Name = "Description",
                Label = "Description",
                DataType = LooseDataType.LongString,
                Required = true,
            },
        };
    }
}
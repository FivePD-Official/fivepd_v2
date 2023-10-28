using System.Collections.Generic;
using FivePD.Server.Interfaces.LooseData;
using FivePD.Server.Models;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Services.LooseData;

/// <inheritdoc />
public class ArrestReportService : LooseDataRepository<ArrestReportSet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArrestReportService"/> class.
    /// </summary>
    /// <param name="looseDataService">The <see cref="ILooseDataService"/> to use.</param>
    public ArrestReportService(ILooseDataService looseDataService)
        : base(looseDataService)
    {
    }

    /// <inheritdoc />
    protected override List<LooseDataAttribute> RegisterSetAttributes()
    {
        return new List<LooseDataAttribute>
        {
            new Models.LooseData.Attributes.DepartmentId(),
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
                Name = "DateOfArrest",
                Label = "Date of arrest",
                DataType = LooseDataType.Date,
                Required = true,
                ShowInGrid = true,
            },
            new()
            {
                Name = "Narrative",
                Label = "Narrative",
                DataType = LooseDataType.LongString,
                Required = true,
            },
            new Models.LooseData.Attributes.PedHistoryIds(),
        };
    }
}
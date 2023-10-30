using System.Collections.Generic;
using FivePD.Server.Interfaces.LooseData;
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
            new CoreAttributes.DepartmentId(new CoreAttributes.Options { Hidden = true }), // TODO: is this needed? if so, why?
            new CoreAttributes.UserId(new CoreAttributes.Options { Hidden = true }),
            new CoreAttributes.SubmitDate(new CoreAttributes.Options { Hidden = true }),
            new CoreAttributes.PedHistoryIds(new CoreAttributes.Options { Hidden = true, Required = true }),
            new()
            {
                Name = "ArrestDate",
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
        };
    }
}
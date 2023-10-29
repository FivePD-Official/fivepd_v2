using System.Collections.Generic;
using FivePD.Server.Interfaces.LooseData;
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
            new Models.LooseData.Attributes.IncidentHistoryId(false),
            new Models.LooseData.Attributes.UserId(true),
            new Models.LooseData.Attributes.SubmitDate(true),
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
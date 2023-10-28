using System.Collections.Generic;
using FivePD.Server.Interfaces.LooseData;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Services.LooseData;

/// <inheritdoc />
public class PedWarningService : LooseDataRepository<ArrestReportSet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PedWarningService"/> class.
    /// </summary>
    /// <param name="looseDataService">The <see cref="ILooseDataService"/> to use.</param>
    public PedWarningService(ILooseDataService looseDataService)
        : base(looseDataService)
    {
    }

    /// <inheritdoc />
    protected override List<LooseDataAttribute> RegisterSetAttributes()
    {
        return new List<LooseDataAttribute>
        {
            new Models.LooseData.Attributes.UserId(),
            new Models.LooseData.Attributes.SubmitDate(),
            new Models.LooseData.Attributes.PedHistoryId(),
            new()
            {
                Name = "Offense",
                Label = "Offense",
                DataType = LooseDataType.LongString,
                Required = true,
            },
            new()
            {
                Name = "Location",
                Label = "Location",
                DataType = LooseDataType.LongString,
                Required = true,
            },
            new()
            {
                Name = "Note",
                Label = "Note",
                DataType = LooseDataType.LongString,
                Required = false,
            },
        };
    }
}
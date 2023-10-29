using System.Collections.Generic;
using FivePD.Server.Interfaces.LooseData;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Services.LooseData;

/// <inheritdoc />
public class PedTicketService : LooseDataRepository<PedTicketSet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PedTicketService"/> class.
    /// </summary>
    /// <param name="looseDataService">The <see cref="ILooseDataService"/> to use.</param>
    public PedTicketService(ILooseDataService looseDataService)
        : base(looseDataService)
    {
    }

    /// <inheritdoc />
    protected override List<LooseDataAttribute> RegisterSetAttributes()
    {
        return new List<LooseDataAttribute>
        {
            new Models.LooseData.Attributes.UserId(true),
            new Models.LooseData.Attributes.SubmitDate(true),
            new Models.LooseData.Attributes.PedHistoryId(false),
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
                Name = "Fine",
                Label = "Fine",
                DataType = LooseDataType.Integer,
                Required = true,
            },
        };
    }
}
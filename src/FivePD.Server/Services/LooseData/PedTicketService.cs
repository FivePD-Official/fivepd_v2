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
            new CoreAttributes.UserId(new CoreAttributes.Options { Hidden = true }),
            new CoreAttributes.SubmitDate(new CoreAttributes.Options { Hidden = true }),
            new CoreAttributes.PedHistoryId(new CoreAttributes.Options { Required = true }),
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
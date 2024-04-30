using System.Collections.Generic;
using FivePD.Server.Interfaces.VariableData;
using FivePD.Server.Models.VariableData;
using FivePD.Server.Models.VariableData.Sets;

namespace FivePD.Server.Services.VariableData;

/// <inheritdoc />
public class PedTicketService : VariableDataRepository<PedTicketSet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PedTicketService"/> class.
    /// </summary>
    /// <param name="variableDataService">The <see cref="IVariableDataService"/> to use.</param>
    public PedTicketService(IVariableDataService variableDataService)
        : base(variableDataService)
    {
    }

    /// <inheritdoc />
    protected override List<VariableDataAttribute> RegisterSetAttributes()
    {
        return new List<VariableDataAttribute>
        {
            new CoreAttributes.UserId(new CoreAttributes.Options { Hidden = true }),
            new CoreAttributes.SubmitDate(new CoreAttributes.Options { Hidden = true }),
            new CoreAttributes.PedHistoryId(new CoreAttributes.Options { Required = true }),
            new()
            {
                Name = "Offense",
                Label = "Offense",
                DataType = VariableDataType.LongString,
                Required = true,
            },
            new()
            {
                Name = "Location",
                Label = "Location",
                DataType = VariableDataType.LongString,
                Required = true,
            },
            new()
            {
                Name = "Fine",
                Label = "Fine",
                DataType = VariableDataType.Integer,
                Required = true,
            },
        };
    }
}
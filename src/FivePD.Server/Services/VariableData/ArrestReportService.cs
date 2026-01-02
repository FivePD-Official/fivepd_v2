using System.Collections.Generic;
using FivePD.Server.Interfaces.VariableData;
using FivePD.Server.Models.VariableData;
using FivePD.Server.Models.VariableData.Sets;

namespace FivePD.Server.Services.VariableData;

/// <inheritdoc />
public class ArrestReportService : VariableDataBaseService<ArrestReportSet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArrestReportService"/> class.
    /// </summary>
    /// <param name="variableDataService">The <see cref="IVariableDataService"/> to use.</param>
    public ArrestReportService(IVariableDataService variableDataService)
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
            new CoreAttributes.PedHistoryIds(new CoreAttributes.Options { Required = true }),
            new()
            {
                Name = "ArrestDate",
                Label = "Date of arrest",
                DataType = VariableDataType.Date,
                Required = true,
                ShowInGrid = true,
            },
            new()
            {
                Name = "Narrative",
                Label = "Narrative",
                DataType = VariableDataType.LongString,
                Required = true,
            },
        };
    }
}
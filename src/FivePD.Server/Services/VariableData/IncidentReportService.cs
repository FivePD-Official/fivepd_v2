using System.Collections.Generic;
using FivePD.Server.Interfaces.VariableData;
using FivePD.Server.Models.VariableData;
using FivePD.Server.Models.VariableData.Sets;

namespace FivePD.Server.Services.VariableData;

/// <inheritdoc />
public class IncidentReportService : VariableDataRepository<IncidentReportSet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentReportService"/> class.
    /// </summary>
    /// <param name="variableDataService">The <see cref="IVariableDataService"/> to use.</param>
    public IncidentReportService(IVariableDataService variableDataService)
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
            new CoreAttributes.IncidentHistoryId(new CoreAttributes.Options { Required = true }),
            new()
            {
                Name = "Description",
                Label = "Description",
                DataType = VariableDataType.LongString,
                Required = true,
            },
        };
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models.VariableData;

/// <summary>
/// Represents a dynamic data set.
/// </summary>
[Table("fivepd_variable_data_sets")]
public class VariableDataSet
{
    /// <summary>
    /// Gets or sets the id of this set.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the entities of this set.
    /// </summary>
    public IList<VariableDataEntity> Entities { get; set; }

    /// <summary>
    /// Gets or sets the attributes of this set.
    /// </summary>
    public IList<VariableDataAttribute> Attributes { get; set; }
}
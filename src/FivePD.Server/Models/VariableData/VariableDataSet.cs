using System.Collections.Generic;

namespace FivePD.Server.Models.VariableData;

/// <summary>
/// Represents a dynamic data set.
/// </summary>
public class VariableDataSet
{
    /// <summary>
    /// Gets or sets the id of this set.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the entities of this set.
    /// </summary>
    public List<VariableDataEntity> Entities { get; set; } = new();

    /// <summary>
    /// Gets or sets the attributes of this set.
    /// </summary>
    public List<VariableDataAttribute> Attributes { get; set; } = new();
}
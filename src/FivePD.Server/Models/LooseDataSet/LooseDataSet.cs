using System.Collections.Generic;

namespace FivePD.Server.Models.LooseDataSet;

/// <summary>
/// Represents a dynamic data set.
/// </summary>
public class LooseDataSet
{
    /// <summary>
    /// Gets or sets the id of this set.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the entities of this set.
    /// </summary>
    public IEnumerable<LooseDataEntity> Entities { get; set; }

    /// <summary>
    /// Gets or sets the attributes of this set.
    /// </summary>
    public IEnumerable<LooseDataAttribute> Attributes { get; set; }
}
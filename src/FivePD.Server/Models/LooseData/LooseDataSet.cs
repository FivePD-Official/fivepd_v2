using System.Collections.Generic;

namespace FivePD.Server.Models.LooseData;

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
    public IList<LooseDataEntity> Entities { get; set; }

    /// <summary>
    /// Gets or sets the attributes of this set.
    /// </summary>
    public IList<LooseDataAttribute> Attributes { get; set; }
}
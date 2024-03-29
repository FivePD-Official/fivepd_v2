using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models.LooseData;

/// <summary>
/// Represents a dynamic data set.
/// </summary>
[Table("fivepd_loose_data_sets")]
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
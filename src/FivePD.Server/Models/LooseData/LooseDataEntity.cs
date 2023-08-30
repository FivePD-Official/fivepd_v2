using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models.LooseData;

/// <summary>
/// Represents a dynamic entity.
/// </summary>
public class LooseDataEntity
{
    /// <summary>
    /// Gets or sets the id of this entity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the <see cref="LooseDataSet"/> which this entity belongs to.
    /// </summary>
    public string SetId { get; set; }

    /// <summary>
    /// Gets or sets the set this entity belongs to.
    /// </summary>
    public LooseDataSet Set { get; set; }

    /// <summary>
    /// Gets or sets the values of this entity.
    /// </summary>
    public IList<LooseDataValue> Values { get; set; }

    /// <summary>
    /// Gets or sets the values of this entity.
    /// If adding or updating an entity this should be used
    /// instead of the <see cref="LooseDataEntity.Values"/> property.
    /// </summary>
    [NotMapped]
    public Dictionary<string, object> MappedValues { get; set; }
}
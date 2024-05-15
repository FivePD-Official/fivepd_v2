using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FivePD.Server.Utilities;

namespace FivePD.Server.Models.VariableData;

/// <summary>
/// Represents a dynamic entity.
/// </summary>
[Table("fivepd_variable_data_entities")]
public class VariableDataEntity
{
    /// <summary>
    /// Gets or sets the id of this entity.
    /// </summary>
    public string Id { get; set; } = IdGenerator.Generate();

    /// <summary>
    /// Gets or sets the id of the <see cref="VariableDataSet"/> which this entity belongs to.
    /// </summary>
    public string SetId { get; set; }

    /// <summary>
    /// Gets or sets the set this entity belongs to.
    /// </summary>
    public VariableDataSet Set { get; set; }

    /// <summary>
    /// Gets or sets the values of this entity.
    /// </summary>
    public IList<VariableDataValue> Values { get; set; }

    /// <summary>
    /// Gets or sets the values of this entity.
    /// If adding or updating an entity this should be used
    /// instead of the <see cref="VariableDataEntity.Values"/> property.
    /// </summary>
    [NotMapped]
    public Dictionary<string, object> MappedValues { get; set; }
}
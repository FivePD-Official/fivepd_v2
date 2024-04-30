using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models.VariableData;

/// <summary>
/// Represents an entity's attribute value.
/// </summary>
[Table("fivepd_variable_data_values")]
public class VariableDataValue
{
    /// <summary>
    /// Gets or sets the id of this value.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the attribute which this value belongs to.
    /// </summary>
    public VariableDataAttribute Attribute { get; set; }

    /// <summary>
    /// Gets or sets the entity which this value belongs to.
    /// </summary>
    public VariableDataEntity Entity { get; set; }

    /// <summary>
    /// Gets or sets the actual value in a serialized format.
    /// </summary>
    public string Value { get; set; }
}
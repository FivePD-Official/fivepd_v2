namespace FivePD.Gamemode.Server.Models.LooseDataSet;

/// <summary>
/// Represents an entity's attribute value.
/// </summary>
public class LooseDataValue
{
    /// <summary>
    /// Gets or sets the id of this value.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the attribute which this value belongs to.
    /// </summary>
    public LooseDataAttribute Attribute { get; set; }

    /// <summary>
    /// Gets or sets the entity which this value belongs to.
    /// </summary>
    public LooseDataEntity Entity { get; set; }

    /// <summary>
    /// Gets or sets the actual value in a serialized format.
    /// </summary>
    public string Value { get; set; }
}
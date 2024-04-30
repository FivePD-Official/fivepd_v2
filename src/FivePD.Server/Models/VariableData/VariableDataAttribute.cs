using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models.VariableData;

/// <summary>
/// Represents a <see cref="VariableDataSet"/> attribute.
/// </summary>
[Table("fivepd_variable_data_attributes")]
public class VariableDataAttribute
{
    /// <summary>
    /// Should be used to specify whether an attribute was created by an admin or not.
    /// </summary>
    public const string AddedByAdminAddonId = "admin";

    /// <summary>
    /// Gets or sets the id of this attribute.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets which set this attribute belongs to.
    /// </summary>
    public VariableDataSet Set { get; set; }

    /// <summary>
    /// Gets or sets the data type of this attribute.
    /// </summary>
    public VariableDataType DataType { get; set; }

    /// <summary>
    /// Gets or sets the id of the addon which created this attribute.
    /// If it was created by an admin it should equal to <see cref="VariableDataAttribute.AddedByAdminAddonId"/>.
    /// </summary>
    public string AddonId { get; set; }

    /// <summary>
    /// Gets or sets the name of this attribute.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the order of this attribute.
    /// Used for overriding the default order of grid columns.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this attribute is required in forms or not.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this attribute should be hidden in forms or not.
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this attribute should be readonly in forms or not.
    /// </summary>
    public bool Readonly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this attribute should be displayed in grids or not.
    /// </summary>
    public bool ShowInGrid { get; set; }

    /// <summary>
    /// Gets or sets the placeholder which will be shown in the attribute's input field.
    /// </summary>
    public string Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the label which will be shown above the attribute's input field.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the help label which will be shown under the attribute's input field.
    /// </summary>
    public string HelpLabel { get; set; }

    /// <summary>
    /// Gets or sets the custom options of this attribute.
    /// </summary>
    public AttributeOptions Options { get; set; }

    /// <summary>
    /// Represents the possible options of an attribute.
    /// </summary>
    public class AttributeOptions
    {
        /// <summary>
        /// Gets or sets the items which will be displayed in dropdown fields.
        /// Only works if the <see cref="VariableDataAttribute.DataType"/> is set to <see cref="VariableDataType.Integer"/>.
        /// </summary>
        public Dictionary<int, string> Items { get; set; }
    }
}
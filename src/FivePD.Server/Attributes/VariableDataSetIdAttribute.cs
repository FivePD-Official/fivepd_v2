using System;

namespace FivePD.Server.Attributes;

/// <summary>
/// Used for handling the dynamic data sets.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class VariableDataSetIdAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDataSetIdAttribute"/> class.
    /// </summary>
    /// <param name="id">The id of the set.</param>
    public VariableDataSetIdAttribute(string id)
    {
        this.Id = id;
    }

    /// <summary>
    /// Gets the id of the set.
    /// </summary>
    public string Id { get; }
}
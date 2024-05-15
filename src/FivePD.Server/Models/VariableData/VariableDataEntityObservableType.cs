namespace FivePD.Server.Models.VariableData;

/// <summary>
/// Defines the possible types of <see cref="VariableDataEntityObservable"/>.
/// </summary>
public enum VariableDataEntityObservableType
{
    /// <summary>
    /// Fires when an entity has been added to a set.
    /// </summary>
    Add,

    /// <summary>
    /// Fires when an entity has been edited in a set.
    /// </summary>
    Update,

    /// <summary>
    /// Fires when an entity has been deleted from a set.
    /// </summary>
    Delete,
}
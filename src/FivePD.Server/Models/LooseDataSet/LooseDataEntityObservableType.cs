namespace FivePD.Server.Models.LooseDataSet;

/// <summary>
/// Defines the possible types of <see cref="LooseDataEntityObservable"/>.
/// </summary>
public enum LooseDataEntityObservableType
{
    /// <summary>
    /// Fires when an entity has been added to a set.
    /// </summary>
    Add,

    /// <summary>
    /// Fires when an entity has been edited in a set.
    /// </summary>
    Edit,

    /// <summary>
    /// Fires when an entity has been deleted from a set.
    /// </summary>
    Delete,
}
namespace FivePD.Common.Models;

/// <summary>
/// The base entity class.
/// </summary>
public class CommonFEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommonFEntity"/> class.
    /// </summary>
    /// <param name="networkId">The entity's network ID.</param>
    protected CommonFEntity(int networkId)
    {
        this.NetworkId = networkId;
    }

    /// <summary>
    /// Gets the entity's network identifier.
    /// </summary>
    public int NetworkId { get; }
}
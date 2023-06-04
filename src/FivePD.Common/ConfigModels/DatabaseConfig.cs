namespace FivePD.Gamemode.Common.ConfigModels;

/// <summary>
/// Contains the database settings.
/// </summary>
public sealed record DatabaseConfig
{
    /// <summary>
    /// Gets the address of the database.
    /// </summary>
    public required string Address { get; init; }

    /// <summary>
    /// Gets the port of the database.
    /// </summary>
    public required string Port { get; init; }

    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public required string Database { get; init; }

    /// <summary>
    /// Gets the user used for connecting to the database.
    /// </summary>
    public required string User { get; init; }

    /// <summary>
    /// Gets the password used for connecting to the database.
    /// </summary>
    public required string Password { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"server={this.Address};port={this.Port};database={this.Database};user={this.User};password={this.Password};";
    }
}
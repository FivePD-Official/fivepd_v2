namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record DatabaseConfig
{
    /// <summary>
    /// Gets the address of the database.
    /// </summary>
    public string Address { get; init; }

    /// <summary>
    /// Gets the port of the database.
    /// </summary>
    public string Port { get; init; }

    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public string DatabaseName { get; init; }

    /// <summary>
    /// Gets the user used for connecting to the database.
    /// </summary>
    public string User { get; init; }

    /// <summary>
    /// Gets the password used for connecting to the database.
    /// </summary>
    public string Password { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"server={this.Address};port={this.Port};database={this.DatabaseName};user={this.User};password={this.Password};";
    }
}
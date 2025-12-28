namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record DatabaseConfig
{
    /// <summary>
    /// Gets the type of the database.
    /// </summary>
    public string Type { get; init; }

    /// <summary>
    /// Gets the address of the database.
    /// </summary>
    public string Address { get; init; }

    /// <summary>
    /// Gets the port of the database.
    /// </summary>
    public uint Port { get; init; }

    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the user used for connecting to the database.
    /// </summary>
    public string User { get; init; }

    /// <summary>
    /// Gets the password used for connecting to the database.
    /// </summary>
    public string Password { get; init; }
    
    public bool IsMysql()
    {
        return this.Type.ToUpper() == "MYSQL";
    }
    
    public bool IsMariaDb()
    {
        return this.Type.ToUpper() == "MARIADB";
    }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return $"server={this.Address};port={this.Port};database={this.Name};user={this.User};password={this.Password};";
    }
}
namespace FivePD.Server.QueryHelpers;

/// <summary>
/// Used for filtering queries in Dapper repositories.
/// </summary>
public class DapperFilter
{
    /// <summary>
    /// Gets or sets a value indicating whether the filter should be applied.
    /// </summary>
    public bool Condition { get; set; }

    /// <summary>
    /// Gets or sets the SQL condition string (without WHERE).
    /// </summary>
    public string SqlCondition { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the SQL condition.
    /// </summary>
    public object Parameters { get; set; }
}
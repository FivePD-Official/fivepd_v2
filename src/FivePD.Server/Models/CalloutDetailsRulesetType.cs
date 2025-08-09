namespace FivePD.Server.Models;

/// <summary>
/// Represents the ruleset type for callout details.
/// </summary>
public enum CalloutDetailsRulesetType
{
    /// <summary>
    /// The callout is allowed.
    /// </summary>
    Allow,

    /// <summary>
    /// The callout is denied.
    /// </summary>
    Deny,
}
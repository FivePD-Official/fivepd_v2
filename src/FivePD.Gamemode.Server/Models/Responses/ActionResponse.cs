using System.Collections.Generic;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Models.Responses;

/// <summary>
/// Used for actions initiated in the NUI.
/// </summary>
internal record struct ActionResponse()
{
    /// <summary>
    /// Gets or sets a value indicating whether the actions has been successful.
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Gets or sets the message if any.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message if any.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the params.
    /// </summary>
    public Dictionary<string, string> Params { get; set; } = new();

    /// <inheritdoc />
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FivePD.Server.Extensions;

/// <summary>
/// Contains extension methods for the <see cref="FPlayer"/>.
/// </summary>
[Obfuscate]
internal static class FPlayerExtensions
{
    /// <summary>
    /// Triggers an event for the underlying player.
    /// </summary>
    /// <param name="player">The player to trigger the specified event for.</param>
    /// <param name="eventName">The event's name.</param>
    /// <param name="data">The data to send.</param>
    public static void TriggerEvent(this FPlayer player, string eventName, JObject data)
    {
        data["Event"] = eventName;
        player.GetUnderlyingPlayer().TriggerEvent(eventName, JsonConvert.SerializeObject(data));
    }
}
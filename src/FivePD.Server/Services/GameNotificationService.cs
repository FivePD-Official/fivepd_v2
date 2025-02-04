using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class GameNotificationService : IGameNotificationService
{
    /// <inheritdoc />
    public void Notify(Player player, GameNotificationOptions options)
    {
        if (player is null)
        {
            return;
        }

        var a = JsonConvert.SerializeObject(options);
        var b = JObject.Parse(a);
        b["Event"] = Events.GameNotification.Notify;
        player.TriggerEvent(Events.GameNotification.Notify, JsonConvert.SerializeObject(b));
    }

    /// <inheritdoc />
    public void Notify(FPlayer player, GameNotificationOptions options)
    {
        if (player is null)
        {
            return;
        }

        this.Notify(player.GetUnderlyingPlayer(), options);
    }
}
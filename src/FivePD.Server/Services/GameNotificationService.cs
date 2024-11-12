using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class GameNotificationService : IGameNotificationService
{
    /// <inheritdoc />
    public void Notify(FPlayer player, GameNotificationOptions options)
    {
        var a = JsonConvert.SerializeObject(options);
        var b = JObject.Parse(a);
        b["Event"] = Events.GameNotification.Notify;
        player.TriggerEvent(Events.GameNotification.Notify, b);
    }
}
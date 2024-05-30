using FivePD.Client.Injection;
using FivePD.Client.Interfaces;
using FivePD.Common;
using FivePD.Common.Utilities;
using Newtonsoft.Json.Linq;

namespace FivePD.Client;

#pragma warning disable

internal static class LifecycleHandler
{
    public static bool IsStopped { get; private set; }

    public static void ServerBootstrapped()
    {
        var uiManagerService = ServiceLocator.Resolve<IUIManagerService>();
        uiManagerService.SendMessage(new JObject
        {
            { "Event", Events.Lifecycle.ServerBootstrapped },
        });
    }

    public static void OnStopped()
    {
        if (IsStopped)
        {
            return;
        }

        var notificationService = ServiceLocator.Resolve<INotificationService>();
        notificationService.ShowNotification(new GtaStyleText()
            .Blue("[FivePD]")
            .NewLine()
            .Red("An error occurred on the server. Stopping FivePD."));

        ServiceLocator.Dispose();

        IsStopped = true;
    }
}
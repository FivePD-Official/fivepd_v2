using FivePD.Client.Injection;
using FivePD.Client.Interfaces;
using FivePD.Common;
using FivePD.Common.Utilities;
using Newtonsoft.Json.Linq;

namespace FivePD.Client;

#pragma warning disable

internal static class LifecycleHandler
{
    public static bool Stopped { get; private set; }

    public static void ServerBootstrapped()
    {
        var uiManagerService = ServiceLocator.Resolve<IUIManagerService>();
        uiManagerService?.SendMessage(new JObject
        {
            { "Event", Events.Lifecycle.ServerBootstrapped },
        });
    }

    public static void Stop(string message)
    {
        if (Stopped)
        {
            return;
        }

        if (!string.IsNullOrEmpty(message))
        {
            var notificationService = ServiceLocator.Resolve<INotificationService>();
            notificationService.ShowNotification(new GtaStyleText()
                .Blue("[FivePD]")
                .NewLine()
                .Red(message));
        }

        ServiceLocator.Dispose();

        Stopped = true;
    }
}
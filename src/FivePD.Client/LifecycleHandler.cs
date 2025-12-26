using FivePD.Client.Injection;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.Utilities;
using Newtonsoft.Json.Linq;

namespace FivePD.Client;

#pragma warning disable

internal static class LifecycleHandler
{
    private static bool _serverBootstrapped;
    private static bool _triggered;

    public static bool Stopped { get; private set; }

    public static void ServerBootstrapped()
    {
        _serverBootstrapped = true;
        TriggerOnServerBootstrapped();
    }

    public static void TriggerOnServerBootstrapped()
    {
        if (_triggered || !_serverBootstrapped || !ServiceLocator.IsInitialized)
        {
            return;
        }

        _triggered = true;

        var uiManagerService = ServiceLocator.Resolve<IUIManagerService>();
        uiManagerService?.SendMessage(new JObject
        {
            { "Event", Events.Lifecycle.ServerBootstrapped },
        });

        var services = ServiceLocator.ResolveAll<IOnServerBootstrapped>();
        foreach (var service in services)
        {
            service.OnServerBootstrapped();
        }
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

            var loggerService = ServiceLocator.Resolve<ILoggerService>();
            loggerService.Error(message);
        }

        ServiceLocator.Dispose();

        Stopped = true;
    }
}
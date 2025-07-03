using System;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Server.Interfaces.IUIService" />
[Obfuscate]
public class UIService : IUIService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IServiceProvider _serviceProvider;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public UIService(
        EventHandlerDictionary eventHandlerDictionary,
        IServiceProvider serviceProvider,
        INetworkEventService networkEventService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._serviceProvider = serviceProvider;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        this.RegisterEventAttributes();
    }

    private void RegisterEventAttributes()
    {
        var methodInfos = Assembly
            .GetExecutingAssembly()
            .GetTypes()
#pragma warning disable S3011
            .SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
#pragma warning restore S3011
            .Where(methodInfo => methodInfo.GetCustomAttribute(typeof(UIEventAttribute), false) != null)
            .ToList();

        foreach (var methodInfo in methodInfos)
        {
            var attribute = methodInfo.GetCustomAttribute<UIEventAttribute>();
            var interfaceType = methodInfo.DeclaringType?
                .GetInterfaces()
                .FirstOrDefault(type => type != typeof(IOnInit) && type != typeof(IOnDestroy));
            var obj = this._serviceProvider.GetService(interfaceType);
            if (obj is null)
            {
                throw new ArgumentException(
                    $"Can't register event method '{attribute.Event}'. Make sure that its declaring type has an interface and has been registered as a service.");
            }

            this._eventHandlerDictionary.Add(
                attribute.Event,
                new Action<Player, string, string>(([FromSource] player, guid, expandoObject) =>
                {
                    this.HandleEvent(obj, methodInfo, player, guid, [player, guid, expandoObject]);
                }));
        }
    }

    private void HandleEvent(object obj, MethodInfo methodInfo, Player player, string guid, object[] parameters)
    {
        if (LifecycleHandler.IsStopped)
        {
            return;
        }

        try
        {
            methodInfo.Invoke(obj, parameters);
        }
        catch (Exception e)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            LifecycleHandler.HandleError(e);
        }
    }
}
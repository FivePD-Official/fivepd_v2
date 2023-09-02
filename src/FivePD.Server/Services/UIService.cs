using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Server.Attributes;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IUIService" />
public class UIService : IUIService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public UIService(
        EventHandlerDictionary eventHandlerDictionary,
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._serviceProvider = serviceProvider;
        this._logger = logger;
    }

    /// <inheritdoc />
    void IServiceWithEvents.RegisterEvents()
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
                .FirstOrDefault(type => type != typeof(IServiceWithEvents));
            var obj = this._serviceProvider.GetService(interfaceType);
            if (obj is null)
            {
                throw new ArgumentException(
                    $"Can't register event method '{attribute.Event}'. Make sure that its declaring type has an interface and has been registered as a service.");
            }

            if (attribute.WithPlayer)
            {
                this._eventHandlerDictionary.Add(
                    attribute.Event,
                    new Action<Player, ExpandoObject, NetworkCallbackDelegate>(([FromSource] player, expandoObject, networkCallbackDelegate) =>
                    {
                        this.HandleEvent(obj, methodInfo, networkCallbackDelegate, new[] { player, (object)expandoObject, networkCallbackDelegate });
                    }));
            }
            else
            {
                this._eventHandlerDictionary.Add(
                    attribute.Event,
                    new Action<ExpandoObject, NetworkCallbackDelegate>((expandoObject, networkCallbackDelegate) =>
                    {
                        this.HandleEvent(obj, methodInfo, networkCallbackDelegate, new[] { (object)expandoObject, networkCallbackDelegate });
                    }));
            }
        }
    }

    private void HandleEvent(object obj, MethodInfo methodInfo, NetworkCallbackDelegate networkCallbackDelegate, object[] parameters)
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
            networkCallbackDelegate.InvokeWithUnexpectedError();
            LifecycleHandler.HandleError(e);
        }
    }
}
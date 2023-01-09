using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IUIService" />
    public class UIService : IUIService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        public UIService(EventHandlerDictionary eventHandlerDictionary, IServiceProvider serviceProvider)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public void RegisterEvents()
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

                var obj = this._serviceProvider.GetService(methodInfo.DeclaringType?.GetInterfaces()[0]);
                if (obj is null)
                {
                    throw new ArgumentException($"Can't register event method '{attribute.Event}'. Make sure that its declaring type has an interface and has been registered as a service.");
                }

                Delegate eventDelegate;
                if (attribute.WithPlayer)
                {
                    eventDelegate = Delegate.CreateDelegate(
                        typeof(Action<Player, ExpandoObject, NetworkCallbackDelegate>), obj, methodInfo);
                }
                else
                {
                    eventDelegate = Delegate.CreateDelegate(typeof(Action<ExpandoObject, NetworkCallbackDelegate>), obj, methodInfo);
                }

                this._eventHandlerDictionary[attribute.Event] += eventDelegate;
            }
        }
    }
}
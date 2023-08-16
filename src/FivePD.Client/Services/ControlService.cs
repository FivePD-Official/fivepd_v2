using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Attributes;
using FivePD.Client.Injection;
using FivePD.Client.Interfaces;
using FivePD.Common;
using FivePD.Common.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable CS0162

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IControlService" />
internal class ControlService : IControlService, IServiceWithEvents
{
    private readonly ITickService _tickService;
    private readonly IUIManagerService _uiService;
    private readonly List<HoldableKeybind> _actions = new();
    private readonly List<string> _registeredControlIds = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlService"/> class.
    /// </summary>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="uiService">The <see cref="IUIManagerService"/> to use.</param>
    public ControlService(ITickService tickService, IUIManagerService uiService)
    {
        this._tickService = tickService;
        this._uiService = uiService;
        this.RegisterServerControls();
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        var methodInfos = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .SelectMany(x => x.GetMethods())
            .ToList();

        var normalKeybinds = methodInfos
            .Where(x => x.GetCustomAttributes(typeof(KeybindAttribute), false).FirstOrDefault() != null)
            .GroupBy(x => x.GetCustomAttribute<KeybindAttribute>().Key);

        foreach (var methodInfoGroup in normalKeybinds)
        {
            var attribute = methodInfoGroup.ElementAt(0).GetCustomAttribute<KeybindAttribute>();
            var keybindActions = new List<Action>();

            foreach (var methodInfo in methodInfoGroup)
            {
                var obj = ServiceLocator.Resolve(methodInfo.DeclaringType);
                keybindActions.Add(() => methodInfo.Invoke(obj, null));
            }

            this.RegisterKeybind(attribute.Id, attribute.Key, attribute.Description, () =>
            {
                foreach (var action in keybindActions)
                {
                    action.Invoke();
                }
            });
        }

        this._tickService.OnTick += this.WaitForHoldAsync;
    }

    /// <inheritdoc />
    public void RegisterKeybind(string id, string key, string title, Action onPress = null, Action onRelease = null)
    {
        this.InternalRegisterKeybind(id, key, title, "KEYBOARD", onPress, onRelease);
    }

    /// <inheritdoc />
    public void RegisterMomentaryKeybind(string id, string key, string title, int timeToHoldInMs, Action onPress = null, Action onRelease = null, Func<bool> canInvokeOnPress = null)
    {
        var holdableKeybind = new HoldableKeybind(onPress, (int)Math.Floor(timeToHoldInMs * 0.05));
        var isInvokable = false;

        void HoldableOnPress()
        {
            isInvokable = canInvokeOnPress?.Invoke() ?? true;
            if (!isInvokable)
            {
                return;
            }

            this._uiService.ShowTimerBar(timeToHoldInMs);
            this._actions.Add(holdableKeybind);
        }

        void HoldableOnRelease()
        {
            if (!isInvokable)
            {
                return;
            }

            holdableKeybind.ClearTimer();
            this._uiService.SendMessage(new JObject
            {
                { "Event", Events.UIEvent.TimerBar },
                { "Show", false },
            });
            this._actions.Remove(holdableKeybind);
        }

        this.InternalRegisterKeybind(id, key, title, "KEYBOARD", HoldableOnPress, HoldableOnRelease);
    }

    /// <inheritdoc />
    public void RegisterMouseKeybind(string id, string key, string title, Action onPress = null, Action onRelease = null)
    {
        this.InternalRegisterKeybind(id, key, title, "MOUSE_BUTTON", onPress, onRelease);
    }

    private void InternalRegisterMomentaryKeybind(ControlsDTO.ClientControlDTO control)
    {
        void OnPress()
        {
            BaseScript.TriggerServerEvent(Events.Control.Trigger, control.Id, "press");
        }

        void OnRelease()
        {
            BaseScript.TriggerServerEvent(Events.Control.Trigger, control.Id, "release");
        }

        this.RegisterMomentaryKeybind(control.Id, control.Key, control.Title, control.TimeToHoldInMs, OnPress, OnRelease);
    }

    private void InternalRegisterKeybind(ControlsDTO.ClientControlDTO control)
    {
        void OnPress()
        {
            BaseScript.TriggerServerEvent(Events.Control.Trigger, control.Id, "press");
        }

        void OnRelease()
        {
            BaseScript.TriggerServerEvent(Events.Control.Trigger, control.Id, "release");
        }

        this.InternalRegisterKeybind(control.Id, control.Key, control.Title, control.KeyMapper, OnPress, OnRelease);
    }

    private void InternalRegisterKeybind(string id, string key, string title, string keyMapper, Action onPress, Action onRelease = null)
    {
        if (this._registeredControlIds.Contains(id))
        {
            return;
        }

        this._registeredControlIds.Add(id);

        Cfx.API.RegisterKeyMapping($"+{id}", title, keyMapper, key);
        Cfx.API.RegisterCommand(
            $"+{id}",
            new Action(() =>
            {
                Cfx.API.CancelEvent();
                onPress?.Invoke();
            }),
            false);
        Cfx.API.RegisterCommand(
            $"-{id}",
            new Action(() =>
            {
                Cfx.API.CancelEvent();
                onRelease?.Invoke();
            }),
            false);
    }

    private void RegisterServerControls()
    {
        BaseScript.TriggerServerEvent(Events.Control.GetAll, new Action<string>(serializedControls =>
        {
            var controls = JsonConvert.DeserializeObject<List<ControlsDTO.ClientControlDTO>>(serializedControls);
            foreach (var control in controls)
            {
                if (control.TimeToHoldInMs > 0)
                {
                    this.InternalRegisterMomentaryKeybind(control);
                    continue;
                }

                this.InternalRegisterKeybind(control);
            }
        }));
    }

    private Task WaitForHoldAsync()
    {
        var toBeRemoved = new List<HoldableKeybind>();
        foreach (var item in this._actions)
        {
            item.IncrementTimer();
            if (item.Timer == item.TimeToHoldInMs)
            {
                item.OnPress?.Invoke();
                toBeRemoved.Add(item);
            }
        }

        foreach (var item in toBeRemoved)
        {
            item.ClearTimer();
            this._uiService.SendMessage(new JObject
            {
                { "Event", Events.UIEvent.TimerBar },
                { "Show", false },
            });
            this._actions.Remove(item);
        }

        return Task.FromResult(0);
    }

    private sealed class HoldableKeybind
    {
        public HoldableKeybind(Action onPress, int timeToHoldInMs)
        {
            this.OnPress = onPress;
            this.TimeToHoldInMs = timeToHoldInMs;
        }

        public Action OnPress { get; }

        public int TimeToHoldInMs { get; }

        public int Timer { get; private set; }

        public void IncrementTimer()
        {
            this.Timer++;
        }

        public void ClearTimer()
        {
            this.Timer = 0;
        }
    }
}
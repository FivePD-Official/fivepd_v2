using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Attributes;
using FivePD.Client.Injection;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable CS0162

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IControlService" />
internal class ControlService : IControlService, IOnInit, IOnDestroy
{
    private readonly ITickService _tickService;
    private readonly IUIManagerService _uiManagerService;
    private readonly List<HoldableKeybind> _actions = new();
    private readonly List<string> _registeredControlIds = new();
    private readonly Dictionary<string, int> _releaseCounts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlService"/> class.
    /// </summary>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="uiManagerService">The <see cref="IUIManagerService"/> to use.</param>
    public ControlService(ITickService tickService, IUIManagerService uiManagerService)
    {
        this._tickService = tickService;
        this._uiManagerService = uiManagerService;
        this.RegisterServerControls();
    }

    /// <inheritdoc />
    public void OnInit()
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

    /// <inheritdoc/>
    public void OnDestroy()
    {
        this._tickService.OnTick -= this.WaitForHoldAsync;
    }

    /// <inheritdoc />
    public void RegisterKeybind(string id, string key, string title, Action onPress = null, Action onRelease = null)
    {
        this.InternalRegisterKeybind(id, key, title, "KEYBOARD", onPress, onRelease);
    }

    /// <inheritdoc />
    public void RegisterMomentaryKeybind(
        string id,
        string key,
        string title,
        int timeToHoldInMs,
        Action onPress = null,
        Action onRelease = null,
        Func<bool> canInvokeOnPress = null)
    {
        var holdableKeybind = new HoldableKeybind(onPress, onRelease, timeToHoldInMs);

        void HoldableOnPress()
        {
            if (!(canInvokeOnPress?.Invoke() ?? true))
            {
                return;
            }

            holdableKeybind.Released = false;
            holdableKeybind.StartTimeMs = Cfx.API.GetGameTimer();

            this._uiManagerService.ShowTimerBar(timeToHoldInMs);
            this._actions.Add(holdableKeybind);
        }

        void HoldableOnRelease()
        {
            holdableKeybind.Released = true;
            this._uiManagerService.SendMessage(new JObject
            {
                { "Event", Events.UIEvent.TimerBar },
                { "Show", false },
            });
            holdableKeybind.OnRelease?.Invoke();
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

        this._releaseCounts[id] = 0;

        void WrappedOnPress()
        {
            this._releaseCounts[id] = 0;
            onPress?.Invoke();
        }

        void WrappedOnRelease()
        {
            this._releaseCounts[id]++;
            if (this._releaseCounts[id] >= 2)
            {
                onRelease?.Invoke();
                this._releaseCounts[id] = 0;
            }
        }

        Cfx.API.RegisterKeyMapping($"+{id}", title, keyMapper, key);
        Cfx.API.RegisterCommand(
            $"+{id}",
            new Action(() =>
            {
                if (LifecycleHandler.Stopped)
                {
                    return;
                }

                Cfx.API.CancelEvent();
                WrappedOnPress();
            }),
            false);
        Cfx.API.RegisterCommand(
            $"-{id}",
            new Action(() =>
            {
                Cfx.API.CancelEvent();
                WrappedOnRelease();
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
            var elapsedTime = Cfx.API.GetGameTimer() - item.StartTimeMs;

            this._uiManagerService.UpdateTimerBarProgress(elapsedTime, item.TargetHoldDurationMs);

            if (elapsedTime >= item.TargetHoldDurationMs)
            {
                item.OnPress?.Invoke();

                this._uiManagerService.SendMessage(new JObject
                {
                    { "Event", Events.UIEvent.TimerBar },
                    { "Show", false },
                });

                toBeRemoved.Add(item);
            }

            if (item.Released)
            {
                toBeRemoved.Add(item);
            }
        }

        foreach (var item in toBeRemoved)
        {
            this._actions.Remove(item);
        }

        return Task.FromResult(0);
    }

    private sealed record HoldableKeybind
    {
        public HoldableKeybind(Action onPress, Action onRelease, int targetHoldDurationMs)
        {
            this.OnPress = onPress;
            this.OnRelease = onRelease;
            this.TargetHoldDurationMs = targetHoldDurationMs;
        }

        public Action OnPress { get; }

        public Action OnRelease { get; }

        public bool Released { get; set; }

        public int TargetHoldDurationMs { get; }

        public int StartTimeMs { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Client.DTO;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Extensions;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services;

/// <inheritdoc cref="FivePD.Gamemode.Client.Interfaces.IConfirmationService" />
internal class ConfirmationService : IConfirmationService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickService _tickService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private string _callbackId;
    private ConfirmationDTO _currentConfirmation;
    private DateTime _actionStart;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmationService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    public ConfirmationService(
        EventHandlerDictionary eventHandlerDictionary,
        ITickService tickService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickService = tickService;
        this._notificationService = notificationService;
        this._localizationService = localizationService;
    }

    /// <inheritdoc/>
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Generic.Action] += new Action<string, string>(this.OnActionTrigger);
        this._tickService.OnTick += this.OnKeypressAsync;
    }

    private void OnActionTrigger(string callbackId, string serializedAction)
    {
        if (this._currentConfirmation is not null)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, this._callbackId, false);
        }

        var action = JsonConvert.DeserializeObject<ConfirmationDTO>(serializedAction);
        this._callbackId = callbackId;
        this._currentConfirmation = action;
        this._actionStart = DateTime.Now;
        if (!string.IsNullOrEmpty(this._currentConfirmation.Tooltip))
        {
            this._notificationService.ShowTooltip(this._localizationService.Get(this._currentConfirmation.Tooltip), true, this._currentConfirmation.TimeoutInMs);
        }

        if (!string.IsNullOrEmpty(this._currentConfirmation.Notification))
        {
            var paramsToReplace = new Dictionary<string, string>();
            if (this._currentConfirmation.NotificationParams is not null)
            {
                foreach (var item in this._currentConfirmation.NotificationParams)
                {
                    paramsToReplace[item.Key] = item.Value;
                }
            }

            paramsToReplace["timeout"] = (this._currentConfirmation.TimeoutInMs / 1000).ToString();

            this._notificationService.ShowNotification(this._localizationService.Get(this._currentConfirmation.Notification).ReplaceParams(paramsToReplace));
        }
    }

    private Task OnKeypressAsync()
    {
        if (this._currentConfirmation is null)
        {
            return Task.FromResult(0);
        }

        var elapsed = DateTime.Now.Subtract(this._actionStart).TotalMilliseconds;
        if (elapsed >= this._currentConfirmation.TimeoutInMs)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, this._callbackId, false);
            this._callbackId = string.Empty;
            this._currentConfirmation = null;
        }
        else
        {
            if (!Cfx.API.IsControlJustPressed(0, this._currentConfirmation.Key))
            {
                return Task.FromResult(0);
            }

            BaseScript.TriggerServerEvent(Events.Generic.Callback, this._callbackId, true);
            this._callbackId = string.Empty;
            this._currentConfirmation = null;
        }

        return Task.FromResult(0);
    }
}
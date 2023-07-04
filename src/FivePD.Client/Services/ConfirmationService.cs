using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Gamemode.Client.DTO;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Extensions;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IConfirmationService" />
internal class ConfirmationService : IConfirmationService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickService _tickService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IControlService _controlService;

    private readonly Queue<QueuedConfirmation> _confirmationQueue = new();
    private QueuedConfirmation _currentConfirmation;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmationService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    public ConfirmationService(
        EventHandlerDictionary eventHandlerDictionary,
        ITickService tickService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IControlService controlService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickService = tickService;
        this._notificationService = notificationService;
        this._localizationService = localizationService;
        this._controlService = controlService;
    }

    /// <inheritdoc/>
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Generic.Action] += new Action<string, string>(this.OnActionTrigger);
        this._tickService.OnTick += this.OnKeypressAsync;
        this._controlService.RegisterKeybind(Controls.ConfirmAction.Id, Controls.ConfirmAction.Key, "Confirms an action.", this.OnConfirm);
    }

    private void OnActionTrigger(string callbackId, string serializedAction)
    {
        var action = JsonConvert.DeserializeObject<ConfirmationDTO>(serializedAction);
        this._confirmationQueue.Enqueue(new QueuedConfirmation(callbackId, action));
    }

    private void OnConfirm()
    {
        if (this._currentConfirmation is null)
        {
            return;
        }

        BaseScript.TriggerServerEvent(Events.Generic.Callback, this._currentConfirmation.CallbackId, true);
        this._currentConfirmation = null;
    }

    private Task OnKeypressAsync()
    {
        try
        {
            if (this._currentConfirmation is null)
            {
                var confirmation = this._confirmationQueue.Dequeue();
                if (confirmation is not null)
                {
                    this._currentConfirmation = confirmation;
                }
            }
        }
        catch (Exception)
        {
            // ignored
        }

        if (this._currentConfirmation is null)
        {
            return Task.FromResult(0);
        }

        if (!this._currentConfirmation.SetActionStart)
        {
            this._currentConfirmation.ActionStart = DateTime.Now;
            this._currentConfirmation.SetActionStart = true;
            this.DisplayConfirmationNotifications(this._currentConfirmation);
        }

        var elapsed = DateTime.Now.Subtract(this._currentConfirmation.ActionStart).TotalMilliseconds;
        if (elapsed >= this._currentConfirmation.Confirmation.TimeoutInMs)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, this._currentConfirmation.CallbackId, false);
            this._currentConfirmation = null;
        }

        return Task.FromResult(0);
    }

    private void DisplayConfirmationNotifications(QueuedConfirmation queuedConfirmation)
    {
        if (!string.IsNullOrEmpty(queuedConfirmation.Confirmation.Properties.Tooltip))
        {
            this._notificationService.ShowTooltip(
                this._localizationService.Get(queuedConfirmation.Confirmation.Properties.Tooltip),
                true,
                queuedConfirmation.Confirmation.TimeoutInMs);
        }

        var notification = queuedConfirmation.Confirmation.Properties.Notification;
        if (notification is null)
        {
            return;
        }

        var paramsToReplace = new Dictionary<string, string>();
        if (notification.ContentParams is not null)
        {
            foreach (var item in notification.ContentParams)
            {
                paramsToReplace[item.Key] = this._localizationService.Get(item.Value);
            }
        }

        paramsToReplace["timeout"] = (queuedConfirmation.Confirmation.TimeoutInMs / 1000).ToString();

        if (string.IsNullOrEmpty(notification.Sender) || string.IsNullOrEmpty(notification.Subject) || string.IsNullOrEmpty(notification.Texture))
        {
            this._notificationService.ShowNotification(
                this._localizationService.Get(notification.Content).ReplaceParams(paramsToReplace));
        }
        else
        {
            this._notificationService.ShowNotificationWithTexture(
                this._localizationService.Get(notification.Texture),
                this._localizationService.Get(notification.Sender),
                this._localizationService.Get(notification.Subject),
                this._localizationService.Get(notification.Content).ReplaceParams(paramsToReplace));
        }
    }

    private sealed record QueuedConfirmation(string CallbackId, ConfirmationDTO Confirmation)
    {
        public bool SetActionStart { get; set; }

        public DateTime ActionStart { get; set; } = DateTime.Now;
    }
}
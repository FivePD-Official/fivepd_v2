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

    private readonly Queue<QueuedConfirmation> _confirmationQueue = new();
    private QueuedConfirmation _currentConfirmation;

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
        var action = JsonConvert.DeserializeObject<ConfirmationDTO>(serializedAction);
        this._confirmationQueue.Enqueue(new QueuedConfirmation(callbackId, action));
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
        }

        this.DisplayConfirmationNotifications(this._currentConfirmation);

        var elapsed = DateTime.Now.Subtract(this._currentConfirmation.ActionStart).TotalMilliseconds;
        Debug.WriteLine("elapsed: " + elapsed + " len: " + this._confirmationQueue.Count);
        if (elapsed >= this._currentConfirmation.Confirmation.TimeoutInMs)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, this._currentConfirmation.CallbackId, false);
            this._currentConfirmation = null;
        }
        else
        {
            if (!Cfx.API.IsControlJustPressed(0, this._currentConfirmation.Confirmation.Key))
            {
                return Task.FromResult(0);
            }

            BaseScript.TriggerServerEvent(Events.Generic.Callback, this._currentConfirmation.CallbackId, true);
            this._currentConfirmation = null;
        }

        return Task.FromResult(0);
    }

    private void DisplayConfirmationNotifications(QueuedConfirmation queuedConfirmation)
    {
        if (!string.IsNullOrEmpty(queuedConfirmation.Confirmation.Tooltip))
        {
            this._notificationService.ShowTooltip(
                this._localizationService.Get(queuedConfirmation.Confirmation.Tooltip),
                true,
                queuedConfirmation.Confirmation.TimeoutInMs);
        }

        if (!string.IsNullOrEmpty(queuedConfirmation.Confirmation.Notification))
        {
            var paramsToReplace = new Dictionary<string, string>();
            if (queuedConfirmation.Confirmation.NotificationParams is not null)
            {
                foreach (var item in queuedConfirmation.Confirmation.NotificationParams)
                {
                    paramsToReplace[item.Key] = item.Value;
                }
            }

            paramsToReplace["timeout"] = (queuedConfirmation.Confirmation.TimeoutInMs / 1000).ToString();

            this._notificationService.ShowNotification(
                this._localizationService.Get(queuedConfirmation.Confirmation.Notification).ReplaceParams(paramsToReplace));
        }
    }

    private sealed record QueuedConfirmation(string CallbackId, ConfirmationDTO Confirmation)
    {
        public bool SetActionStart { get; set; }

        public DateTime ActionStart { get; set; } = DateTime.Now;
    }
}
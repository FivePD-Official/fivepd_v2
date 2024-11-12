using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.Models;
using Newtonsoft.Json;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IConfirmationService" />
internal class ConfirmationService : IConfirmationService, IOnInit, IOnDestroy
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickService _tickService;
    private readonly IControlService _controlService;
    private readonly IUIManagerService _uiManagerService;
    private readonly IGameNotificationService _gameNotificationService;

    private readonly Queue<QueuedConfirmation> _confirmationQueue = new();
    private QueuedConfirmation _currentConfirmation;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmationService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    /// <param name="controlService">The <see cref="IControlService"/> to use.</param>
    /// <param name="uiManagerService">The <see cref="IUIManagerService"/> to use.</param>
    /// <param name="gameNotificationService">The <see cref="IGameNotificationService"/> to use.</param>
    public ConfirmationService(
        EventHandlerDictionary eventHandlerDictionary,
        ITickService tickService,
        IControlService controlService,
        IUIManagerService uiManagerService,
        IGameNotificationService gameNotificationService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickService = tickService;
        this._controlService = controlService;
        this._uiManagerService = uiManagerService;
        this._gameNotificationService = gameNotificationService;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary[Events.Generic.Action] += new Action<string, string>(this.OnActionTrigger);
        this._tickService.OnTick += this.OnKeypressAsync;
        this._controlService.RegisterKeybind(Controls.ConfirmAction.Id, Controls.ConfirmAction.Key, "Confirms an action.", this.OnConfirm);
    }

    /// <inheritdoc/>
    public void OnDestroy()
    {
        this._tickService.OnTick -= this.OnKeypressAsync;
    }

    private void OnActionTrigger(string callbackId, string serializedAction)
    {
        var action = JsonConvert.DeserializeObject<ConfirmationProperties>(serializedAction);
        this._confirmationQueue.Enqueue(new QueuedConfirmation(callbackId, action));
    }

    private void OnConfirm()
    {
        if (this._uiManagerService.WelcomeModalOpen)
        {
            this._uiManagerService.ToggleView(Events.UIEvent.WelcomeModal);
            BaseScript.TriggerServerEvent(Events.Player.DismissedWelcomeModal);
            return;
        }

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
        if (elapsed >= this._currentConfirmation.ConfirmationOptions.Timeout)
        {
            BaseScript.TriggerServerEvent(Events.Generic.Callback, this._currentConfirmation.CallbackId, false);
            this._currentConfirmation = null;
        }

        return Task.FromResult(0);
    }

    private void DisplayConfirmationNotifications(QueuedConfirmation queuedConfirmation)
    {
            this._gameNotificationService.Notify(queuedConfirmation.ConfirmationOptions);
    }

    private sealed record QueuedConfirmation(string CallbackId, ConfirmationProperties ConfirmationOptions)
    {
        public bool SetActionStart { get; set; }

        public DateTime ActionStart { get; set; } = DateTime.Now;
    }
}
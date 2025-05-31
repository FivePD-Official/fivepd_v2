using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IAccessControlService" />
[Obfuscate]
public class AccessControlService : IAccessControlService, IOnInit
{
    private readonly DatabaseContext _databaseContext;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IPlayerHandlerService _playerHandlerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessControlService"/> class.
    /// </summary>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="PlayerHandlerService"/> to use.</param>
    public AccessControlService(
        DatabaseContext databaseContext,
        EventHandlerDictionary eventHandlerDictionary,
        IPlayerHandlerService playerHandlerService)
    {
        this._databaseContext = databaseContext;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._playerHandlerService = playerHandlerService;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.AccessControl.HasAccess] += new Action<Player, NetworkCallbackDelegate>(this.HasAccess);
    }

    /// <inheritdoc />
    public void GiveAccess(User user)
    {
        this.ToggleAccess(user, true);
    }

    /// <inheritdoc />
    public void RemoveAccess(User user)
    {
        this.ToggleAccess(user, false);
    }

    private void ToggleAccess(User user, bool access)
    {
        if (user is null)
        {
            return;
        }

        user.AccessFivePD = access;
        this._databaseContext.SaveChanges();

        var player = this._playerHandlerService.FindPlayerOfUser(user);
        player?.TriggerEvent(access ? Events.AccessControl.GiveAccess : Events.AccessControl.RemoveAccess);
    }

    private void HasAccess([FromSource] Player player, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var user = this._playerHandlerService.FindUserOfPlayer(player);
        if (user is null)
        {
            networkCallbackDelegate.Invoke(true);
            return;
        }

        networkCallbackDelegate.Invoke(user.AccessFivePD);
    }

    [UIEvent(Events.AccessControl.GiveAccessUI)]
    private void AllowUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var user = this._databaseContext.Users.FirstOrDefault(x => x.Id == request.Id);
        if (user is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        this.GiveAccess(user);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CHANGE",
            "UI.LABEL.ACCESS_CONTROL.SUCCESS.GIVE_ACCESS",
            new Dictionary<string, string>
            {
                { "name", user.Name },
            });
    }

    [UIEvent(Events.AccessControl.RemoveAccessUI)]
    private void DisallowUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var user = this._databaseContext.Users.FirstOrDefault(x => x.Id == request.Id);
        if (user is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        this.RemoveAccess(user);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CHANGE",
            "UI.LABEL.ACCESS_CONTROL.SUCCESS.REMOVE_ACCESS",
            new Dictionary<string, string>
            {
                { "name", user.Name },
            });
    }
}
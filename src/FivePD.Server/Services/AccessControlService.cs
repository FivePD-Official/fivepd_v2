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

#pragma warning disable

public class AccessControlService : IAccessControlService, IOnInit
{
    private readonly DatabaseContext _databaseContext;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IPlayerHandlerService _playerHandlerService;

    public AccessControlService(
        DatabaseContext databaseContext,
        EventHandlerDictionary eventHandlerDictionary,
        IPlayerHandlerService playerHandlerService)
    {
        this._databaseContext = databaseContext;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._playerHandlerService = playerHandlerService;
    }

    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.AccessControl.IsAllowed] += new Action<Player, NetworkCallbackDelegate>(this.OnIsAllowed);
    }

    private void OnIsAllowed([FromSource] Player player, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var user = this._playerHandlerService.FindUserOfPlayer(player);
        if (user is null)
        {
            networkCallbackDelegate.Invoke(true);
            return;
        }

        networkCallbackDelegate.Invoke(this.IsAllowed(user));
    }

    public bool IsAllowed(User user)
    {
        return user is null || user.AccessFivePD;
    }

    public void Allow(User user)
    {
        this.ToggleAccess(user, true);
    }

    public void Disallow(User user)
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
        player?.TriggerEvent(access ? Events.AccessControl.Allow : Events.AccessControl.Disallow);
    }

    [UIEvent(Events.AccessControl.AllowUI)]
    private void AllowUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var user = this._databaseContext.Users.FirstOrDefault(x => x.Id == request.Id);
        if (user is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        this.Allow(user);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CHANGE",
            "UI.LABEL.ACCESS_CONTROL.SUCCESS.GIVE_ACCESS",
            new Dictionary<string, string>
            {
                { "name", user.Name }
            });
    }

    [UIEvent(Events.AccessControl.DisallowUI)]
    private void DisallowUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var user = this._databaseContext.Users.FirstOrDefault(x => x.Id == request.Id);
        if (user is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        this.Disallow(user);
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CHANGE",
            "UI.LABEL.ACCESS_CONTROL.SUCCESS.REMOVE_ACCESS",
            new Dictionary<string, string>
            {
                { "name", user.Name }
            });
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class MessageService : IMessageService
{
    private readonly IDatabaseContextFactory _dbContextFactory;
    private readonly IInternalAddonMetadataService _addonMetadataService;
    private readonly IPlayerHandlerService _playerHandlerService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly INetworkEventService _networkEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageService"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDatabaseContextFactory"/> to use.</param>
    /// <param name="addonMetadataService">The <see cref="IAddonMetadataService"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="IPlayerHandlerService"/> to use.</param>
    /// <param name="gameNotificationService">The <see cref="IGameNotificationService"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public MessageService(
        IDatabaseContextFactory dbContextFactory,
        IAddonMetadataService addonMetadataService,
        IPlayerHandlerService playerHandlerService,
        IGameNotificationService gameNotificationService,
        INetworkEventService networkEventService)
    {
        this._dbContextFactory = dbContextFactory;
        this._addonMetadataService = (IInternalAddonMetadataService)addonMetadataService;
        this._playerHandlerService = playerHandlerService;
        this._gameNotificationService = gameNotificationService;
        this._networkEventService = networkEventService;
    }

    /// <inheritdoc />
    public Message Send(User to, string subject, string content, string sender = "")
    {
        if (to is null)
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(to));
        }

        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(subject));
        }

        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(content));
        }

        if (string.IsNullOrEmpty(sender))
        {
            var addonDefinition = this._addonMetadataService.GetExecutingAddonDefinition();
            sender = addonDefinition?.Name ?? "Unknown";
        }

        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var entry = databaseContext.Messages.Add(new Message
        {
            User = to,
            Subject = subject,
            Content = content,
            Sender = sender,
            SentAt = DateTime.Now,
        });
        databaseContext.SaveChanges();

        this.NotifyUser(to, entry.Entity);

        return entry.Entity;
    }

    /// <inheritdoc />
    public Message Send(Player to, string subject, string content, string sender = "")
    {
        var user = this._playerHandlerService.FindUserOfPlayer(to);
        return this.Send(user, subject, content, sender);
    }

    private void NotifyUser(User user, Message message)
    {
        var player = this._playerHandlerService.FindPlayerOfUser(user);
        if (player is null)
        {
            return;
        }

        this._gameNotificationService.Notify(
            player,
            new GameNotificationOptions
            {
                Title = "FivePD",
                Message = "LABEL.MESSAGES.RECEIVED",
                MessageParams = new Dictionary<string, string>
                {
                    ["sender"] = message.Sender,
                },
            });
    }

    [UIEvent(Events.Message.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);

        var user = this._playerHandlerService.FindUserOfPlayer(player);
        if (user is null)
        {
            return;
        }

        var message = databaseContext.Messages
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, message);
    }

    [UIEvent(Events.Message.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<MessageGetAllPageableRequest>(serializedRequest);

        var user = this._playerHandlerService.FindUserOfPlayer(player);
        if (user is null)
        {
            return;
        }

        var filters = new Filters<Message>();
        var page = databaseContext.Messages
            .AsNoTracking()
            .Where(u => u.User.Id == user.Id)
            .OrderByDescending(x => x.SentAt)
            .Paginate(request.Page, filters, 6);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.Message.Send)]
    private void SendUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<SendMessageRequest>(serializedRequest);
        var sender = this._playerHandlerService.FindUserOfPlayer(player);
        if (sender is null)
        {
            return;
        }

        var users = databaseContext.Users;
        var to = request.SendToEveryone ? users.Where(x => x.Id != sender.Id) : users.Where(x => request.To.Contains(x.Id));
        foreach (var user in to.ToList())
        {
            this.Send(user, request.Subject, request.Content, sender.Name);
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.SEND", "LABEL.MESSAGES.SUCCESS.SEND");
    }

    [UIEvent(Events.Message.Read)]
    private void ReadUI([FromSource] Player player, string guid, string serializedRequest)
    {
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);

        var user = this._playerHandlerService.FindUserOfPlayer(player);
        if (user is null)
        {
            return;
        }

        var message = databaseContext.Messages.FirstOrDefault(x => x.Id == request.Id);
        if (message is not null)
        {
            message.Read = true;
            databaseContext.SaveChanges();
        }

        this._networkEventService.TriggerEventSerialized(player, guid, message);
    }
}
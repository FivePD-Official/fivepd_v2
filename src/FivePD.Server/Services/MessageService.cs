using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using Dapper;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Database.Sql;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc />
[Obfuscate]
public class MessageService : IMessageService
{
    private readonly IInternalAddonMetadataService _addonMetadataService;
    private readonly IPlayerHandlerService _playerHandlerService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly INetworkEventService _networkEventService;
    private readonly MessageRepository _messageRepository;
    private readonly IDapperDatabaseConnectionFactory _dbConnectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageService"/> class.
    /// </summary>
    /// <param name="addonMetadataService">The <see cref="IAddonMetadataService"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="IPlayerHandlerService"/> to use.</param>
    /// <param name="gameNotificationService">The <see cref="IGameNotificationService"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    /// <param name="dbDatabaseConnectionFactory">The <see cref="IDapperDatabaseConnectionFactory"/> to use.</param>
    public MessageService(
        IAddonMetadataService addonMetadataService,
        IPlayerHandlerService playerHandlerService,
        IGameNotificationService gameNotificationService,
        INetworkEventService networkEventService,
        MessageRepository messageRepository,
        IDapperDatabaseConnectionFactory dbDatabaseConnectionFactory)
    {
        this._addonMetadataService = (IInternalAddonMetadataService)addonMetadataService;
        this._playerHandlerService = playerHandlerService;
        this._gameNotificationService = gameNotificationService;
        this._networkEventService = networkEventService;
        this._messageRepository = messageRepository;
        this._dbConnectionFactory = dbDatabaseConnectionFactory;
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

        var message = new Message
        {
            Sender = sender,
            Subject = subject,
            Content = content,
            SentAt = DateTime.Now,
            HasRead = false,
            UserId = to.Id,
        };

        var savedMessage = this._messageRepository.Add(message);
        this.NotifyUser(to, savedMessage);

        return savedMessage;
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
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var message = this._messageRepository.FindById(request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, message);
    }

    [UIEvent(Events.Message.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<MessageGetAllPageableRequest>(serializedRequest);
        var page = this._messageRepository.Paginate(request.Page);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.Message.Send)]
    private void SendUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<SendMessageRequest>(serializedRequest);
        var sender = this._playerHandlerService.FindUserOfPlayer(player);
        if (sender is null)
        {
            return;
        }

        var builderResult = SqlBuilder
            .From(TableNames.Users)
            .If(request.SendToEveryone, b => b.Where("Id != @Id", new { Id = sender.Id }))
            .If(!request.SendToEveryone, b => b.WhereIn("Id", request.To.Select(x => x)))
            .Build();
        using var connection = this._dbConnectionFactory.CreateScope().Connection;
        var users = connection.Query<User>(builderResult.Sql, builderResult.Parameters);
        foreach (var user in users)
        {
            this.Send(user, request.Subject, request.Content, sender.Name);
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.SEND", "LABEL.MESSAGES.SUCCESS.SEND");
    }

    [UIEvent(Events.Message.Read)]
    private void ReadUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var message = this._messageRepository.FindById(request.Id);
        if (message is not null)
        {
            this._messageRepository.Update(message, x => { x.HasRead = true; });
        }

        this._networkEventService.TriggerEventSerialized(player, guid, message);
    }
}
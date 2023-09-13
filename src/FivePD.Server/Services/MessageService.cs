using System;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Addons;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class MessageService : IMessageService
{
    private readonly DatabaseContext _databaseContext;
    private readonly IInternalAddonMetadataService _addonMetadataService;
    private readonly IPlayerDataService _playerDataService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageService"/> class.
    /// </summary>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="addonMetadataService">The <see cref="IAddonMetadataService"/> to use.</param>
    /// <param name="playerDataService">The <see cref="IPlayerDataService"/> to use.</param>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    public MessageService(
        DatabaseContext databaseContext,
        IAddonMetadataService addonMetadataService,
        IPlayerDataService playerDataService,
        INotificationService notificationService)
    {
        this._databaseContext = databaseContext;
        this._addonMetadataService = (IInternalAddonMetadataService)addonMetadataService;
        this._playerDataService = playerDataService;
        this._notificationService = notificationService;
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

        var entry = this._databaseContext.Messages.Add(new Message
        {
            User = to,
            Subject = subject,
            Content = content,
            Sender = sender,
            SentAt = DateTime.Now,
        });
        this._databaseContext.SaveChanges();

        this.NotifyUser(to, entry.Entity);

        return entry.Entity;
    }

    /// <inheritdoc />
    public Message Send(Player to, string subject, string content, string sender = "")
    {
        var user = this._playerDataService.FindUserOfPlayer(to);
        return this.Send(user, subject, content, sender);
    }

    private void NotifyUser(User user, Message message)
    {
        var player = this._playerDataService.FindPlayerOfUser(user);
        if (player is null)
        {
            return;
        }

        this._notificationService.ShowNotification(FPlayer.FromPlayer(player), $"You've received a new message from ~b~{message.Sender}");

        // TODO: send message to player and show in UI
    }

    [UIEvent(Events.Message.Get, true)]
    private void GetUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();

        var user = this._playerDataService.FindUserOfPlayer(player);
        if (user is null)
        {
            return;
        }

        var message = this._databaseContext.Messages
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == request.Id);
        networkCallbackDelegate.InvokeWithJson(message);
    }

    [UIEvent(Events.Message.GetAllPageable, true)]
    private void GetAllPageableUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<MessageGetAllPageableRequest>();

        var user = this._playerDataService.FindUserOfPlayer(player);
        if (user is null)
        {
            return;
        }

        var filters = new Filters<Message>();
        var page = this._databaseContext.Messages
            .AsNoTracking()
            .Where(u => u.User.Id == user.Id)
            .OrderByDescending(x => x.SentAt)
            .Paginate(request.Page, filters, 6);

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.Message.Send, true)]
    private void SendUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<SendMessageRequest>();
            var sender = this._databaseContext.Users
                .AsNoTracking()
                .WhereAnyIdMatches(player)
                .FirstOrDefault();
            if (sender is null)
            {
                return;
            }

            var to = this._databaseContext.Users.Where(x => request.To.Contains(x.Id)).ToList();
            foreach (var user in to)
            {
                this.Send(user, request.Subject, request.Content, sender.Name);
            }

            networkCallbackDelegate.InvokeWithResponse(true, "ok", "ok");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            networkCallbackDelegate.InvokeWithUnexpectedError();
        }
    }

    [UIEvent(Events.Message.Read, true)]
    private void ReadUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();

        var user = this._playerDataService.FindUserOfPlayer(player);
        if (user is null)
        {
            return;
        }

        var message = this._databaseContext.Messages.FirstOrDefault(x => x.Id == request.Id);
        if (message is not null)
        {
            message.Read = true;
            this._databaseContext.SaveChangesAsync();
        }

        networkCallbackDelegate.InvokeWithJson(null);
    }
}
using System;
using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class MessageUpdate : ChangeTracker
{
    private string? _sender;
    private string? _subject;
    private string? _content;
    private DateTime _sentAt;
    private bool _hasRead;
    private int _userId;

    public string? Sender
    {
        get => this._sender;
        set => this.SetField(ref this._sender, value, nameof(Sender));
    }

    public string? Subject
    {
        get => this._subject;
        set => this.SetField(ref this._subject, value, nameof(Subject));
    }

    public string? Content
    {
        get => this._content;
        set => this.SetField(ref this._content, value, nameof(Content));
    }

    public DateTime SentAt
    {
        get => this._sentAt;
        set => this.SetField(ref this._sentAt, value, nameof(SentAt));
    }

    public bool HasRead
    {
        get => this._hasRead;
        set => this.SetField(ref this._hasRead, value, nameof(this.HasRead));
    }

    public int UserId
    {
        get => this._userId;
        set => this.SetField(ref this._userId, value, nameof(UserId));
    }
}
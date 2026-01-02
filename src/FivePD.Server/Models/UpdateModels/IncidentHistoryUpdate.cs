using System;
using FivePD.Common.Enums;
using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class IncidentHistoryUpdate : ChangeTracker
{
    private string? _incidentId;
    private string? _title;
    private string? _description;
    private string? _location;
    private FPriority _priority;
    private FIncidentType _type;
    private DateTime _generatedAt;

    public string? IncidentId
    {
        get => this._incidentId;
        set => this.SetField(ref this._incidentId, value, nameof(IncidentId));
    }

    public string? Title
    {
        get => this._title;
        set => this.SetField(ref this._title, value, nameof(Title));
    }

    public string? Description
    {
        get => this._description;
        set => this.SetField(ref this._description, value, nameof(Description));
    }

    public string? Location
    {
        get => this._location;
        set => this.SetField(ref this._location, value, nameof(Location));
    }

    public FPriority Priority
    {
        get => this._priority;
        set => this.SetField(ref this._priority, value, nameof(Priority));
    }

    public FIncidentType Type
    {
        get => this._type;
        set => this.SetField(ref this._type, value, nameof(Type));
    }

    public DateTime GeneratedAt
    {
        get => this._generatedAt;
        set => this.SetField(ref this._generatedAt, value, nameof(GeneratedAt));
    }
}
using System;
using FivePD.Common.Enums;
using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class PedDataUpdate : ChangeTracker
{
    private FGender _gender;
    private string? _firstname;
    private string? _lastname;
    private DateTime _birthdate;
    private double _alcoholLevel;

    public FGender Gender
    {
        get => this._gender;
        set => this.SetField(ref this._gender, value, nameof(Gender));
    }

    public string? Firstname
    {
        get => this._firstname;
        set => this.SetField(ref this._firstname, value, nameof(Firstname));
    }

    public string? Lastname
    {
        get => this._lastname;
        set => this.SetField(ref this._lastname, value, nameof(Lastname));
    }

    public DateTime Birthdate
    {
        get => this._birthdate;
        set => this.SetField(ref this._birthdate, value, nameof(Birthdate));
    }

    public double AlcoholLevel
    {
        get => this._alcoholLevel;
        set => this.SetField(ref this._alcoholLevel, value, nameof(AlcoholLevel));
    }
}
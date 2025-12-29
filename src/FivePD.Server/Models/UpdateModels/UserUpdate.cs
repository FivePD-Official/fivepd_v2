using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class UserUpdate : ChangeTracker
{
    private bool _accessFivePD;
    private string? _preferredLanguage;
    private bool _firstLogin;
    private bool _dismissedWelcomeMessage;
    private string? _name;
    private string? _identifierLicense;
    private string? _identifierDiscord;
    private string? _identifierSteam;
    private string? _identifierXbl;
    private string? _identifierLive;
    private int _xp;
    private double _audioVolume;
    private int _role;
    private string? _callsign;
    private int? _userCallsignFormatId;
    private int? _departmentId;
    private int? _divisionId;
    private int? _rankId;
    private bool _fixedRank;

    public bool AccessFivePD
    {
        get => this._accessFivePD;
        set => this.SetField(ref this._accessFivePD, value, nameof(AccessFivePD));
    }

    public string? PreferredLanguage
    {
        get => this._preferredLanguage;
        set => this.SetField(ref this._preferredLanguage, value, nameof(PreferredLanguage));
    }

    public bool FirstLogin
    {
        get => this._firstLogin;
        set => this.SetField(ref this._firstLogin, value, nameof(FirstLogin));
    }

    public bool DismissedWelcomeMessage
    {
        get => this._dismissedWelcomeMessage;
        set => this.SetField(ref this._dismissedWelcomeMessage, value, nameof(DismissedWelcomeMessage));
    }

    public string? Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value, nameof(Name));
    }

    public string? IdentifierLicense
    {
        get => this._identifierLicense;
        set => this.SetField(ref this._identifierLicense, value, nameof(IdentifierLicense));
    }

    public string? IdentifierDiscord
    {
        get => this._identifierDiscord;
        set => this.SetField(ref this._identifierDiscord, value, nameof(IdentifierDiscord));
    }

    public string? IdentifierSteam
    {
        get => this._identifierSteam;
        set => this.SetField(ref this._identifierSteam, value, nameof(IdentifierSteam));
    }

    public string? IdentifierXbl
    {
        get => this._identifierXbl;
        set => this.SetField(ref this._identifierXbl, value, nameof(IdentifierXbl));
    }

    public string? IdentifierLive
    {
        get => this._identifierLive;
        set => this.SetField(ref this._identifierLive, value, nameof(IdentifierLive));
    }

    public int Xp
    {
        get => this._xp;
        set => this.SetField(ref this._xp, value, nameof(Xp));
    }

    public double AudioVolume
    {
        get => this._audioVolume;
        set => this.SetField(ref this._audioVolume, value, nameof(AudioVolume));
    }

    public int Role
    {
        get => this._role;
        set => this.SetField(ref this._role, value, nameof(Role));
    }

    public string? Callsign
    {
        get => this._callsign;
        set => this.SetField(ref this._callsign, value, nameof(Callsign));
    }

    public int? UserCallsignFormatId
    {
        get => this._userCallsignFormatId;
        set => this.SetField(ref this._userCallsignFormatId, value, nameof(UserCallsignFormatId));
    }

    public int? DepartmentId
    {
        get => this._departmentId;
        set => this.SetField(ref this._departmentId, value, nameof(DepartmentId));
    }

    public int? DivisionId
    {
        get => this._divisionId;
        set => this.SetField(ref this._divisionId, value, nameof(DivisionId));
    }

    public int? RankId
    {
        get => this._rankId;
        set => this.SetField(ref this._rankId, value, nameof(RankId));
    }

    public bool FixedRank
    {
        get => this._fixedRank;
        set => this.SetField(ref this._fixedRank, value, nameof(FixedRank));
    }
}
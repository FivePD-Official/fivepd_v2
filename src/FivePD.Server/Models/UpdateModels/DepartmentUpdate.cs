using System.Collections.Generic;
using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class DepartmentUpdate : ChangeTracker
{
    private string _name;
    private string _callsign;
    private bool _useGlobalRanks;
    private int _maxUsers;
    private int? _defaultRankId;
    private List<CallsignFormatItem> _callsignFormat;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value, nameof(Name));
    }

    public string Callsign
    {
        get => _callsign;
        set => SetField(ref _callsign, value, nameof(Callsign));
    }

    public bool UseGlobalRanks
    {
        get => _useGlobalRanks;
        set => SetField(ref _useGlobalRanks, value, nameof(UseGlobalRanks));
    }

    public int MaxUsers
    {
        get => _maxUsers;
        set => SetField(ref _maxUsers, value, nameof(MaxUsers));
    }

    public int? DefaultRankId
    {
        get => _defaultRankId;
        set => SetField(ref _defaultRankId, value, nameof(DefaultRankId));
    }
    
    public List<CallsignFormatItem> CallsignFormat
    {
        get => _callsignFormat;
        set => SetField(ref _callsignFormat, value, nameof(CallsignFormat));
    }
}
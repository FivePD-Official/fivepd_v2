using System.Collections.Generic;
using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class UserCallsignFormatUpdate : ChangeTracker
{
    private List<string>? _values;

    public List<string>? Values
    {
        get => this._values;
        set => this.SetField(ref this._values, value, nameof(Values));
    }
}
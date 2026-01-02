using System.Collections.Generic;
using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class UserCallsignFormatUpdate : ChangeTracker
{
    private List<string>? _formatValues;

    public List<string>? FormatValues
    {
        get => this._formatValues;
        set => this.SetField(ref this._formatValues, value, nameof(FormatValues));
    }
}
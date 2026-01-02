using FivePD.Server.Database;

namespace FivePD.Server.Models.UpdateModels;

#pragma warning disable
#nullable enable
public class VehicleDataUpdate : ChangeTracker
{
    private int? _pedDataId;
    private string? _licensePlate;
    private string? _color;
    private string? _brand;

    public int? PedDataId
    {
        get => this._pedDataId;
        set => this.SetField(ref this._pedDataId, value, nameof(PedDataId));
    }

    public string? LicensePlate
    {
        get => this._licensePlate;
        set => this.SetField(ref this._licensePlate, value, nameof(LicensePlate));
    }

    public string? Color
    {
        get => this._color;
        set => this.SetField(ref this._color, value, nameof(Color));
    }

    public string? Brand
    {
        get => this._brand;
        set => this.SetField(ref this._brand, value, nameof(Brand));
    }
}
namespace FivePD.Gamemode.Common.ConfigModels;

#pragma warning disable

public sealed record GeneralSettings
{
    public UnitSettings Unit { get; init; }
    
    public sealed record UnitSettings
    {
        public string Join { get; init; }

        public bool JoinFreely => this.Join.ToUpper() == "FREE";

        public bool JoinByRequest => this.Join.ToUpper() == "REQUEST";
    }
}
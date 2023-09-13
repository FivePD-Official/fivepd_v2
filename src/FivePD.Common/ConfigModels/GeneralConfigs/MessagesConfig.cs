namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record MessagesConfig
{
    public string CanSend { get; init; } = "ALL";
    
    public bool CanSendAll => this.CanSend.ToUpper() == "ALL";

    public bool CanSendAdmins => this.CanSend.ToUpper() == "ADMINS";
}
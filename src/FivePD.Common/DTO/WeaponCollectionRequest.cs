using FivePD.Common.Attributes;

namespace FivePD.Gamemode.Client.Requests;

#pragma warning disable

[NoUsage]
internal class WeaponCollectionRequest
{
    public int NetworkId { get; set; }
    
    public string SubEvent { get; set; }
    
    public string Args { get; set; }
}
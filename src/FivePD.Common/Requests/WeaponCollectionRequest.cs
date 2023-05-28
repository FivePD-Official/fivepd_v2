namespace FivePD.Gamemode.Client.Requests;

#pragma warning disable

public class WeaponCollectionRequest
{
    public int NetworkId { get; set; }
    
    public string SubEvent { get; set; }
    
    public string Args { get; set; }
}
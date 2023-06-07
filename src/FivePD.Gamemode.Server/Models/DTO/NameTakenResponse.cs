using Newtonsoft.Json;

#pragma warning disable

namespace FivePD.Gamemode.Server.Models.DTO;

internal readonly record struct NameTakenResponse(bool Taken)
{
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
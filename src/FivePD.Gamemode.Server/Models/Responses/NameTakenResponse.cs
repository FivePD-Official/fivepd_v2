using Newtonsoft.Json;

#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Responses;

public readonly record struct NameTakenResponse(bool Taken)
{
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
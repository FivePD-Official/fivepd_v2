#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

internal class DeleteEntitiesRequest
{
    public enum DeleteEntityType
    {
        All,
        Peds,
        Vehicles,
        Props,
    }

    public DeleteEntityType Type { get; set; }
}
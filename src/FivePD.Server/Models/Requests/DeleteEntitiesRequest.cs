namespace FivePD.Server.Models.Requests;

#pragma warning disable

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
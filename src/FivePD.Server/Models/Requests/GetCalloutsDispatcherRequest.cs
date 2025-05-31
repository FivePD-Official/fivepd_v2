namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct GetCalloutsDispatcherRequest
{
    public bool HasUnitsAssigned { get; init; }
}
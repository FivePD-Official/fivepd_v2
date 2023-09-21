namespace FivePD.Server.Models;

#pragma warning disable

public sealed record Drug(string Name)
{
    public string Name { get; set; }
}
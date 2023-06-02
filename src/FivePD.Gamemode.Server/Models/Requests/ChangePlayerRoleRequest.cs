#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

public record struct ChangePlayerRoleRequest
{
    public int UserId { get; set; }

    public int[] Roles { get; set; }
}
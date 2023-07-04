#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct ChangePlayerRoleRequest
{
    public int UserId { get; set; }

    public int[] Roles { get; set; }
}
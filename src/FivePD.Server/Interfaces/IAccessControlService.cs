using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IAccessControlService
{
    public bool IsAllowed(User user);

    public void Allow(User user);

    public void Disallow(User user);
}
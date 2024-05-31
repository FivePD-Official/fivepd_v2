using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides methods for handling the access of users to FivePD.
/// </summary>
public interface IAccessControlService
{
    /// <summary>
    /// Gives access to FivePD to the specified user.
    /// </summary>
    /// <param name="user">The user to give access to.</param>
    public void GiveAccess(User user);

    /// <summary>
    /// Removes access of the specified user from using FivePD.
    /// </summary>
    /// <param name="user">The user to remove access from.</param>
    public void RemoveAccess(User user);
}
using System.Collections.Generic;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Interfaces.Dispatcher;

#pragma warning disable

/// <summary>
/// Service responsible for managing storage and retrieval of dispatcher information.
/// </summary>
public interface IDispatcherStorageService
{
    /// <summary>
    /// Gets a list of all active dispatchers in the system.
    /// </summary>
    /// <returns>A list of FPlayer objects representing active dispatchers.</returns>
    public List<FPlayer> Dispatchers();
}
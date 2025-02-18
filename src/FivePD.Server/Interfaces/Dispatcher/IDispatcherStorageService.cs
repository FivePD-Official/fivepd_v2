using System.Collections.Generic;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Interfaces.Dispatcher;

/// <summary>
/// Provides storage and management functionality for dispatcher-related data.
/// </summary>
public interface IDispatcherStorageService
{
    /// <summary>
    /// Gets a list of all active dispatchers.
    /// </summary>
    /// <returns>A list of players currently acting as dispatchers.</returns>
    public List<FPlayer> Dispatchers();
}
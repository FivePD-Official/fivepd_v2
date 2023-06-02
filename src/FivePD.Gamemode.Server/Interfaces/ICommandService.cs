using System;
using System.Collections.Generic;
using FivePD.Gamemode.Server.Models.CommandModels;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Responsible for the global "fivepd" command.
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Register a command under "/fivepd".
    /// </summary>
    /// <param name="name">Name of the command.</param>
    /// <param name="function">The method that'll be invoked upon entering the command.</param>
    /// <param name="canInvoke">The command's function will only be invoked if this method returns true.</param>
    /// <returns>The created command.</returns>
    public Command RegisterCommand(string name, Action<int, List<object>> function, Func<bool> canInvoke = null);
}
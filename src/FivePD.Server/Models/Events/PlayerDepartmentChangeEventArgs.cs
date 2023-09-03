namespace FivePD.Server.Models.Events;

/// <summary>
/// Provides additional information regarding a player's
/// department change, both when they join or leave one.
/// </summary>
/// <param name="User">The user.</param>
/// <param name="Department">The department.</param>
public sealed record PlayerDepartmentChangeEventArgs(User User, Department Department);
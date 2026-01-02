using System;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for handling all instances of <see cref="Rank"/>.
/// </summary>
public interface IRankService
{
    /// <summary>
    /// Fires when a rank has been created.
    /// </summary>
    public event EventHandler<Rank> RankCreated;

    /// <summary>
    /// Fires when a rank has been edited.
    /// </summary>
    public event EventHandler<Rank> RankUpdated;

    /// <summary>
    /// Fires when a rank has been deleted.
    /// </summary>
    public event EventHandler<Rank> RankDeleted;
}
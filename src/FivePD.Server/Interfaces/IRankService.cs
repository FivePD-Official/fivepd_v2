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

    /// <summary>
    /// Adds the given rank to the database.
    /// </summary>
    /// <param name="rank">The rank to create.</param>
    /// <returns>The created rank entity with an Id.</returns>
    public Rank CreateRank(Rank rank);

    /// <summary>
    /// Updated the fields of the given rank.
    /// </summary>
    /// <param name="rank">The rank to update.</param>
    /// <returns>The updated rank.</returns>
    public Rank UpdateRank(Rank rank);

    /// <summary>
    /// Removes the given rank.
    /// </summary>
    /// <param name="rank">The rank to remove.</param>
    public void DeleteRank(Rank rank);
}
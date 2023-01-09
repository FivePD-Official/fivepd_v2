using System;
using System.Collections.Generic;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.LooseDataSet;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Provides easy access to reportable entities through <see cref="ILooseDataService"/>.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Fires when an entity is added to <see cref="ArrestReportSet"/> either via the API or the NUI.
        /// </summary>
        /// <param name="observer">The action that will executed upon firing with the added entity.</param>
        /// <returns>A disposable <see cref="LooseDataEntityObservableSubscription"/>.</returns>
        public IDisposable OnArrestReportAdd(Action<LooseDataEntity> observer);

        /// <summary>
        /// Fires when an entity is edited in <see cref="ArrestReportSet"/> either via the API or the NUI.
        /// </summary>
        /// <param name="observer">The action that will executed upon firing with the edited entity.</param>
        /// <returns>A disposable <see cref="LooseDataEntityObservableSubscription"/>.</returns>
        public IDisposable OnArrestReportEdit(Action<LooseDataEntity> observer);

        /// <summary>
        /// Fires when an entity is deleted from <see cref="ArrestReportSet"/> either via the API or the NUI.
        /// </summary>
        /// <param name="observer">The action that will executed upon firing with the deleted entity.</param>
        /// <returns>A disposable <see cref="LooseDataEntityObservableSubscription"/>.</returns>
        public IDisposable OnArrestReportDelete(Action<LooseDataEntity> observer);

        /// <summary>
        /// Inserts an entity into <see cref="ArrestReportSet"/>.
        /// </summary>
        /// <param name="arrestReport">The entity to insert.</param>
        /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
        public void AddArrestReport(LooseDataEntity arrestReport);

        /// <summary>
        /// Updates the given entity with its values.
        /// </summary>
        /// <param name="arrestReport">The entity to update.</param>
        /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
        public void UpdateArrestReport(LooseDataEntity arrestReport);

        /// <summary>
        /// Deletes the given entity.
        /// </summary>
        /// <param name="arrestReport">The entity to delete.</param>
        /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
        public void DeleteArrestReport(LooseDataEntity arrestReport);

        /// <summary>
        /// Finds and maps the first entity with the given id.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        /// <returns>The first entity with the given id.</returns>
        /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
        public LooseDataEntity GetArrestReport(int id);

        /// <summary>
        /// Finds and maps all entities of <see cref="ArrestReportSet"/>.
        /// </summary>
        /// <returns>All entities of the set.</returns>
        /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
        public List<LooseDataEntity> GetArrestReports();
    }
}
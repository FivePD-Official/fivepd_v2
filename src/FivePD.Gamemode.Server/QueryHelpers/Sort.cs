using System;
using System.Linq.Expressions;

#pragma warning disable SA1649

namespace FivePD.Gamemode.Server.QueryHelpers
{
    /// <summary>
    /// Used for sorting queries.
    /// </summary>
    /// <typeparam name="T">Type of Entity for which the sort is applicable.</typeparam>
    /// <typeparam name="TKey">The data type for the sort object.</typeparam>
    public class Sort<T, TKey>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the sort should be applied.
        /// </summary>
        public bool Condition { get; set; }

        /// <summary>
        /// Gets or sets the expression which will be used for sorting the query.
        /// </summary>
        public Expression<Func<T, TKey>> Expression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sorting
        /// should be in descending or ascending order.
        /// </summary>
        public bool ByDescending { get; set; }

        /// <summary>
        /// Gets or sets whether this sort has priority over others.
        /// </summary>
        public int Priority { get; set; }
    }
}
// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Linq;

#pragma warning disable SA1649

namespace FivePD.Gamemode.Server.QueryHelpers
{
    /// <summary>
    /// Contains extension methods for <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Paginates your query and returns Page object for the given page number and page size.
        /// </summary>
        /// <typeparam name="T">Type of Entity for which pagination is being implemented.</typeparam>
        /// <param name="query">IQueryable on which pagination will be applied.</param>
        /// <param name="pageNumber">The page no. which needs to be fetched.</param>
        /// <param name="sorts">Conditional sorts.</param>
        /// <returns>A Page object with filtered data for the given page number and page size.</returns>
        public static Page<T> Paginate<T>(this IQueryable<T> query, int pageNumber, Sorts<T> sorts)
        {
            return query.ApplySort(sorts).Paginate(pageNumber);
        }

        /// <summary>
        /// Paginates your query and returns Page object for the given page number and page size.
        /// </summary>
        /// <typeparam name="T">Type of Entity for which pagination is being implemented.</typeparam>
        /// <param name="query">IQueryable on which pagination will be applied.</param>
        /// <param name="pageNumber">The page no. which needs to be fetched.</param>
        /// <param name="filters">Conditional filters.</param>
        /// <returns>A Page object with filtered data for the given page number and page size.</returns>
        public static Page<T> Paginate<T>(this IQueryable<T> query, int pageNumber, Filters<T> filters)
        {
            return query.ApplyFilter(filters).Paginate(pageNumber);
        }

        /// <summary>
        /// Paginates your query and returns Page object for the given page number and page size.
        /// </summary>
        /// <typeparam name="T">Type of Entity for which pagination is being implemented.</typeparam>
        /// <param name="query">IQueryable on which pagination will be applied.</param>
        /// <param name="pageNumber">The page no. which needs to be fetched.</param>
        /// <param name="sorts">Conditional sorts.</param>
        /// <param name="filters">Conditional filters.</param>
        /// <returns>A Page object with filtered data for the given page number and page size.</returns>
        public static Page<T> Paginate<T>(this IQueryable<T> query, int pageNumber, Sorts<T> sorts, Filters<T> filters)
        {
            return query.ApplyFilter(filters).Paginate(pageNumber, sorts);
        }

        private static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, Filters<T> filters)
        {
            return !filters.IsValid() ? query : filters.Get().Aggregate(query, (current, filter) => current.Where(filter.Expression));
        }

        private static IQueryable<T> ApplySort<T>(this IQueryable<T> query, Sorts<T> sorts)
        {
            return !sorts.IsValid() ? query : Sorts<T>.ApplySorts(query, sorts);
        }

        /// <summary>
        /// Paginates your query and returns Page object for the given page number and page size.
        /// Note: OrderBy is mandatory for the pagination to work.
        /// </summary>
        /// <typeparam name="T">Type of Entity for which pagination is being implemented.</typeparam>
        /// <param name="query">IQueryable on which pagination will be applied.</param>
        /// <param name="pageNumber">The page no. which needs to be fetched.</param>
        /// <param name="pageSize">The number or records expected in the page.</param>
        /// <returns>A Page object with filtered data for the given page number and page size.</returns>
        private static Page<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize = 10)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            var result = new Page<T>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                ItemCount = query.Count(),
                Items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            };
            result.PageCount = (int)Math.Ceiling((double)result.ItemCount / pageSize);
            return result;
        }
    }
}
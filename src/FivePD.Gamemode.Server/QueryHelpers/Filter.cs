// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Linq.Expressions;

#pragma warning disable SA1649

namespace FivePD.Gamemode.Server.QueryHelpers
{
    /// <summary>
    /// Used for filtering queries.
    /// </summary>
    /// <typeparam name="T">Type of Entity for which the filters are applicable.</typeparam>
    public class Filter<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the expression should be applied or not.
        /// </summary>
        public bool Condition { get; set; }

        /// <summary>
        /// Gets or sets the expression which will be applied to the query.
        /// </summary>
        public Expression<Func<T, bool>> Expression { get; set; }
    }
}
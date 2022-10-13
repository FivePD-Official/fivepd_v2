// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Linq.Expressions;
using FivePD.Common.Models;
using FivePD.Common.UIModels;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Handles the UI menus.
    /// </summary>
    public interface IMenuService
    {
        /// <summary>
        /// Registers a menu of the given <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The menu type.</typeparam>
        public void Register<T>()
            where T : Menu;

        /// <summary>
        /// asdasd.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <param name="menuItem">Expr.</param>
        /// <param name="property">Exprws.</param>
        /// <param name="value">Value.</param>
        /// <typeparam name="TI">T.</typeparam>
        public void UpdateMenuItem<TI>(FPlayer player, Expression<Func<MenuItem>> menuItem, Expression<Func<MenuItem, object>> property, object value)
            where TI : Menu;
    }
}
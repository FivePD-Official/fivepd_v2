// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Responsible for managing <see cref="Department"/> objects.
    /// </summary>
    public interface IDepartmentService
    {
        /// <summary>
        /// Registers all <see cref="Department"/> related events.
        /// </summary>
        public void RegisterEvents();
    }
}
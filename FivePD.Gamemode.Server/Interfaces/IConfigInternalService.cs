// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Common.ConfigModels;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Provides utility methods for the FivePD resource itself.
    /// Used for only internal methods.
    /// </summary>
    internal interface IConfigInternalService
    {
        /// <summary>
        /// Loads and returns the database connection settings.
        /// </summary>
        /// <returns>Returns the database connection settings.</returns>
        public DatabaseConfig GetDatabaseConfig();
    }
}
// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

#pragma warning disable SA1618

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Provides utility methods for the FivePD resource itself.
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// Loads a file's content, validates it,
        /// If empty, returns T with its default values.
        /// Otherwise returns the data converted to the given type.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The data converted to T.</returns>
        public T LoadFile<T>(string path);
    }
}
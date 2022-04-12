// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Threading.Tasks;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Responsible for handling traffic stops.
    /// </summary>
    public interface ITrafficStopService
    {
        /// <summary>
        /// Initiate a traffic stop.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Initiate();

        /// <summary>
        /// Cancel the current traffic stop.
        /// </summary>
        public void Cancel();
    }
}
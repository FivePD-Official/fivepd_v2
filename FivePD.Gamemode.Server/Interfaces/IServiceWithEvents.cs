// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Common.Attributes;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Services which require events in order to function properly
    /// should inherit this interface.
    /// </summary>
    [NoUsage]
    public interface IServiceWithEvents
    {
        /// <summary>
        /// Registers all events related to the current class.
        /// </summary>
        void RegisterEvents();
    }
}
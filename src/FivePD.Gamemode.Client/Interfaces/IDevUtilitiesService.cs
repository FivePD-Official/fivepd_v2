// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides functionality related
    /// to gamemode development.
    /// </summary>
    public interface IDevUtilitiesService
    {
        /// <summary>
        /// Prints a development message to
        /// the screen for a brief period of time.
        /// </summary>
        /// <param name="message">The text to show.</param>
        public void ShowDevelopmentMessage(string message);
    }
}
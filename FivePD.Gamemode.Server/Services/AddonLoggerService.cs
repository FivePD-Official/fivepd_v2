// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Gamemode.Server.Interfaces;
using Serilog;
using Serilog.Core;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc />
    public class AddonLoggerService : IAddonLoggerService
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddonLoggerService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public AddonLoggerService(ILogger logger)
        {
            this._logger = logger.ForContext("SourceIdentifier", "AnAddon");
        }

        /// <inheritdoc />
        public void Information(string message)
        {
            this._logger.Information(message);
        }
    }
}
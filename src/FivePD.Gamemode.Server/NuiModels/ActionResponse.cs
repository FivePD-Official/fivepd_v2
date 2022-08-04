// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.NuiModels
{
    /// <summary>
    /// Used for actions initiated in the NUI.
    /// </summary>
    public class ActionResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the actions has been successful.
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// Gets or sets the message if any.
        /// Should be set to a localization path.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
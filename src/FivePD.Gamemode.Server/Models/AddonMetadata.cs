using System.Collections.Generic;

namespace FivePD.Gamemode.Server.Models
{
    /**
     * <summary>Similar to <see cref="AddonDefinition" />, but provided by a user.
     * May contain malformed data.</summary>
     */
    public class AddonMetadata
    {
        /**
         * <summary>Gets or sets the friendly name for the addon. Should be Title Case.</summary>
         */
        public string FriendlyName { get; set; }

        /**
         * <summary>Gets or sets the machine-readable version for the addon.
         * A higher value indicates a newer version.</summary>
         */
        public int Version { get; set; }

        /**
         * <summary>Gets or sets the friendly version for the addon.</summary>
         */
        public string FriendlyVersion { get; set; }

        /**
         * <summary>Gets or sets a list of assemblies found in the current folder to attempt to load.</summary>
         */
        public IList<string> Assemblies { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets files.
        /// </summary>
        public IList<string> Files { get; set; } = new List<string>();
    }
}
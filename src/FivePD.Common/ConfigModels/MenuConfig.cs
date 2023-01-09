using FivePD.Common.ConfigModels.MenuConfigs;

namespace FivePD.Common.ConfigModels
{
    /// <summary>
    /// Contains the config of all official menus.
    /// </summary>
    public class MenuConfig
    {
        /// <summary>
        /// Gets or sets the duty menu config.
        /// </summary>
        public DutyMenuConfig DutyMenu { get; set; } = new DutyMenuConfig();
    }
}
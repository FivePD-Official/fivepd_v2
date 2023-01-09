#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Common.Localization
{
    /// <summary>
    /// Contains all keys that are used within Duty menu.
    /// </summary>
    public class DutyMenuLocalization
    {
        public string Title { get; set; } = "Duty menu";

        public MenuItemLocalization OnDuty { get; set; } = new MenuItemLocalization("On duty", "Description of on duty.");

        public MenuItemLocalization GetLoadout { get; set; } = new MenuItemLocalization("Get loadout", "Description of get loadout");

        public MenuItemLocalization ChangeLanguage { get; set; } = new MenuItemLocalization("Change language", "Description of change language");

        public MenuItemLocalization SpawnVehicle { get; set; } = new MenuItemLocalization("Spawn vehicle", "Description of spawn vehicle");
    }
}
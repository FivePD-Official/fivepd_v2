namespace FivePD.Common;

#pragma warning disable

internal sealed record Controls
{
    // Menus
    public static readonly Control VehicleSelectorMenu = new("fivepd::vehicleSelectorMenu", "F10", "Vehicle selector menu");

    public static readonly Control LoadoutSelectorMenu = new("fivepd::loadoutSelectorMenu", "F11", "Loadout selector menu");

    public static readonly Control DispatchMenu = new("fivepd::dispatchMenu", "F4", "Dispatch menu");

    public static readonly Control AssistanceMenu = new("fivepd::assistanceMenu", "F5", "Assistance menu");

    public static readonly Control InteractionMenu = new("fivepd::interactionMenu", "X", "Interaction menu");

    public static readonly Control SceneManagerMenu = new("fivepd::sceneManagerMenu", "F9", "UI.MENU.SCENE_MANAGER.TITLE");

    // Other
    public static readonly Control TryAlertEntity = new("fivepd::tryAlertEntityAsync", "E", "Try to alert entity", 1000);

    public static readonly Control TryAlertVehicle = new("fivepd::tryAlertVehicleAsync", "LSHIFT", "Try to alert a vehicle", 1000);

    public static readonly Control ConfirmAction = new("fivepd::confirmAction", "F1", "Confirms an action");

    public static readonly Control OpenMDT = new("fivepd::openMDT", "B", "Open MDT");

    public static readonly Control OpenInformation = new("fivepd::openInformation", "I", "Open information panel");

    public static readonly Control ToggleALPR = new Control("fivepd::toggleALPR", "F9", "Toggle ALPR");

    public sealed record Control(string Id, string Key, string Title = "", int TimeToHoldInMs = 0)
    {
        public string Id { get; private set; } = Id;

        public string Key { get; private set; } = Key;

        public string Title { get; private set; } = Title;

        public int TimeToHoldInMs { get; private set; } = TimeToHoldInMs;
    }
}
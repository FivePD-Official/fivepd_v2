namespace FivePD.Common;

#pragma warning disable

internal sealed record Controls
{
    // Menus
    public static readonly Control VehicleSelectorMenu = new("fivepd::vehicleSelectorMenu", "F10", "UI.MENU.VEHICLE.TITLE");

    public static readonly Control LoadoutSelectorMenu = new("fivepd::loadoutSelectorMenu", "F11", "UI.MENU.LOADOUT.TITLE");

    public static readonly Control DispatchMenu = new("fivepd::dispatchMenu", "F4", "UI.MENU.DISPATCH.TITLE");

    public static readonly Control AssistanceMenu = new("fivepd::assistanceMenu", "F5", "UI.MENU.ASSISTANCE.TITLE");

    public static readonly Control InteractionMenu = new("fivepd::interactionMenu", "X", "UI.MENU.INTERACTION.TITLE");

    public static readonly Control SceneManagerMenu = new("fivepd::sceneManagerMenu", "F9", "UI.MENU.SCENE_MANAGER.TITLE");

    public static readonly Control QuestioningMenu = new("fivepd::questioningMenu", "Q", "UI.MENU.QUESTIONING.TITLE");

    // Other
    public static readonly Control TryAlertEntity = new("fivepd::tryAlertEntityAsync", "E", "Try to alert entity", 1000);

    public static readonly Control TryAlertVehicle = new("fivepd::tryAlertVehicleAsync", "LSHIFT", "Try to alert a vehicle", 1000);

    public static readonly Control ConfirmAction = new("fivepd::confirmAction", "F1", "Confirms an action");

    public static readonly Control OpenMDT = new("fivepd::openMDT", "B", "Open MDT");

    public static readonly Control OpenInformation = new("fivepd::openInformation", "I", "Open information panel");

    public static readonly Control ToggleAlpr = new("fivepd::toggleALPR", "F7", "Toggle ALPR");

    public sealed record Control(string Id, string Key, string Title = "", int TimeToHoldInMs = 0)
    {
        public string Id { get; private set; } = Id;

        public string Key { get; private set; } = Key;

        public string Title { get; private set; } = Title;

        public int TimeToHoldInMs { get; private set; } = TimeToHoldInMs;
    }
}
namespace FivePD.Common;

#pragma warning disable

internal sealed record Controls
{
    // Menus
    public static readonly Control VehicleSelectorMenu = new("fivepd::vehicleSelectorMenu", "F10", "LABEL.MENU.VEHICLE.TITLE");

    public static readonly Control LoadoutSelectorMenu = new("fivepd::loadoutSelectorMenu", "F11", "LABEL.MENU.LOADOUT.TITLE");

    public static readonly Control DispatchMenu = new("fivepd::dispatchMenu", "F4", "LABEL.MENU.DISPATCH.TITLE");

    public static readonly Control AssistanceMenu = new("fivepd::assistanceMenu", "F5", "LABEL.MENU.ASSISTANCE.TITLE");

    public static readonly Control InteractionMenu = new("fivepd::interactionMenu", "X", "LABEL.MENU.INTERACTION.TITLE");

    public static readonly Control SceneManagerMenu = new("fivepd::sceneManagerMenu", "F9", "LABEL.MENU.SCENE_MANAGER.TITLE");

    public static readonly Control QuestioningMenu = new("fivepd::questioningMenu", "Q", "LABEL.MENU.QUESTIONING.TITLE");

    // Other
    public static readonly MomentaryControl EnterVehicleAsPassenger = new("fivepd::enterVehicleAsPassenger", "G", "Enter vehicle as passenger", "Entering vehicle as passenger...", 500);

    public static readonly MomentaryControl TryAlertEntity = new("fivepd::tryAlertEntityAsync", "E", "Try to alert entity", "Alerting ped...", 500);

    public static readonly MomentaryControl TryAlertVehicle = new("fivepd::tryAlertVehicleAsync", "LSHIFT", "Try to alert a vehicle", "Alerting vehicle...", 500);

    public static readonly Control ConfirmationAccept = new("fivepd::confirmationAccept", "F1", "Confirms an action");

    public static readonly Control ConfirmationDeny = new("fivepd::confirmationDeny", "F2", "Denies an action");

    public static readonly Control OpenMDT = new("fivepd::openMDT", "B", "Open MDT");

    public static readonly Control OpenInformation = new("fivepd::openInformation", "I", "Open information panel");

    public static readonly Control ToggleAlpr = new("fivepd::toggleALPR", "F7", "Toggle ALPR");

    public record Control(string Id, string Key, string Title)
    {
        public string Id { get; private set; } = Id;

        public string Key { get; private set; } = Key;

        public string Title { get; private set; } = Title;

    }

    public record MomentaryControl(string Id, string Key, string Title, string TimerBarTitle, int TimeToHoldInMs) : Control(Id, Key, Title)
    {
        public string TimerBarTitle { get; private set; } = TimerBarTitle;

        public int TimeToHoldInMs { get; private set; } = TimeToHoldInMs;
    }
}

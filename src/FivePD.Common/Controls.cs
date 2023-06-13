namespace FivePD.Gamemode.Common;

#pragma warning disable

internal sealed record Controls
{
    public static readonly Control TryAlertEntityAsync = new("fivepd::tryAlertEntityAsync", "E");

    public static readonly Control ConfirmAction = new("fivepd::confirmAction", "F1");

    public static readonly Control DutyMenu = new("fivepd::dutyMenu", "F10", "Duty menu");

    public static readonly Control AssistanceMenu = new("fivepd::assistanceMenu", "F4", "Assistance menu");

    public sealed record Control(string Id, string Key, string Title = "", string Description = "")
    {
        public string Id { get; private set; } = Id;

        public string Key { get; private set; } = Key;

        public string Title { get; private set; } = Title;

        public string Description { get; private set; } = Description;
    }
}
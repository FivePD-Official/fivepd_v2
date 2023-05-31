namespace FivePD.Common;

#pragma warning disable

public sealed record Controls
{
    public static readonly Control TryAlertEntityAsync = new("fivepd::tryAlertEntityAsync", "E");

    public record struct Control(string Id, string Key, string Description = "")
    {
        public string Id { get; private set; } = Id;

        public string Key { get; private set; } = Key;

        public string Description { get; private set; } = Description;
    }
}
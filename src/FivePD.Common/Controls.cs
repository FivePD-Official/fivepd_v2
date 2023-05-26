namespace FivePD.Common;

#pragma warning disable

public class Controls
{
    public static readonly Control TryAlertEntityAsync = new("fivepd::tryAlertEntityAsync", "E");

    public sealed class Control
    {
        public string Id { get; private set; }

        public string Key { get; private set; }

        public string Description { get; private set; }

        public Control(string id, string key, string description = "")
        {
            this.Id = id;
            this.Key = key;
            this.Description = description;
        }
    }
}
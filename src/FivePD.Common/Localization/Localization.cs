#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Common.Localization
{
    public class Localization
    {
        public string Title { get; set; } = "English";

        public DutyMenuLocalization DutyMenu { get; set; } = new();

        public TrafficStopLocalization TrafficStop { get; set; } = new();

        public ErrorLocalizations Error { get; set; } = new();

        public WarningLocalizations Warning { get; set; } = new();

        public NuiLocalization Nui { get; set; } = new();

        public PlayerStatusLocalization PlayerStatus { get; set; } = new();
    }
}
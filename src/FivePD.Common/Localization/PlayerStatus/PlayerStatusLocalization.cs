#pragma warning disable CS1591
#pragma warning disable SA1600
#pragma warning disable SA1402

namespace FivePD.Common.Localization
{
    public class PlayerStatusLocalization
    {
        public OnDutyLocalization OnDuty { get; set; } = new OnDutyLocalization();

        public OffDutyLocalization OffDuty { get; set; } = new OffDutyLocalization();
    }
}
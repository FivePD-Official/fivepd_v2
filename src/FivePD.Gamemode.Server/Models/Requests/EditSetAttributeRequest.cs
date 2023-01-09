using FivePD.Gamemode.Server.Models.LooseDataSet;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.Requests
{
    public class EditSetAttributeRequest
    {
        public string SetId { get; set; }

        public LooseDataAttribute Attribute { get; set; }
    }
}
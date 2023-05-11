using FivePD.Gamemode.Server.Models.LooseDataSet;

#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests
{
    public record struct AddSetAttributeRequest
    {
        public string SetId { get; set; }

        public LooseDataAttribute Attribute { get; set; }
    }
}
using System.Dynamic;

#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.Requests
{
    public class AddEntityRequest
    {
        public string SetId { get; set; }

        public int EntityId { get; set; }

        public ExpandoObject Entity { get; set; }
    }
}
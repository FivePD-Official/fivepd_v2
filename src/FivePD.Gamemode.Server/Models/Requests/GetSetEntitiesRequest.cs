#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.Requests
{
    public class GetSetEntitiesRequest
    {
        public string SetId { get; set; }

        public int Page { get; set; }

        public RequestFilters Filters { get; set; } = new RequestFilters();

        public class RequestFilters
        {
        }
    }
}
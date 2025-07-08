#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct AssistanceServiceGetTypePageRequest()
{
    public int Page { get; set; }

    public RequestFilters Filters { get; set; } = default;

    public record struct RequestFilters
    {
        public string TypeName { get; set; }

        public string AssemblyName { get; set; }
    }
}
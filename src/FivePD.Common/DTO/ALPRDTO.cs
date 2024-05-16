namespace FivePD.Common.DTO
{
    internal sealed record AlprDto
    {
        public record struct AlprDTO
        {
            public bool WantedStatus { get; set; }
            public string Data { get; set; }
        }
    }
}
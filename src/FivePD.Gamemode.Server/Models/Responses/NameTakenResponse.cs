using Newtonsoft.Json;

#pragma warning disable CS1591
#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Gamemode.Server.Models.Responses
{
    public class NameTakenResponse
    {
        public NameTakenResponse(bool taken)
        {
            this.Taken = taken;
        }

        public bool Taken { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
using FivePD.Gamemode.Client.Interfaces;

namespace FivePD.Gamemode.Client.Services
{
    public class FakeService : IFakeService
    {
        public string ReturnAString()
        {
            return "Service provider is working.";
        }
    }
}
using FivePD.Gamemode.Client.Interfaces;

namespace FivePD.Gamemode.Client.Services
{
    public class AnotherFakeService : IAnotherFakeService
    {
        private IFakeService _fakeService;
        
        public AnotherFakeService(IFakeService fakeService)
        {
            _fakeService = fakeService;
        }

        public string ReturnAnotherString()
        {
            return $"I asked IFakeService for a string and got: {_fakeService.ReturnAString()}";
        }
    }
}
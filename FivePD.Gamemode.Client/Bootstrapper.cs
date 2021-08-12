using CitizenFX.Core;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Injection;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Services;
using API = CitizenFX.Core.Native.API;

namespace FivePD.Gamemode.Client
{
    public class Bootstrapper : BaseScript
    {
        public Bootstrapper()
        {
            var services = new ServiceProviderBuilder();
            
            services
                .Register(() => EventHandlers)
                .As<EventHandlerDictionary>();

            services
                .RegisterType<FakeService>()
                .IsSingleton()
                .As<IFakeService>();
            
            services
                .RegisterType<AnotherFakeService>()
                .IsSingleton()
                .As<IAnotherFakeService>();

            var provider = services.Build();

            var fakeService = provider.Resolve<IFakeService>();
            var anotherFakeService = provider.Resolve<IAnotherFakeService>();
            
            Debug.WriteLine("Hello from FivePD (Client)!");
            
            Debug.WriteLine($"Response from IFakeService: {fakeService.ReturnAString()}");
            Debug.WriteLine($"Response from IAnotherFakeService: {anotherFakeService.ReturnAnotherString()}");
        }
        
        [Command("open")]
        public void MenuOpen()
        {
            API.SendNuiMessage("{\"type\":\"open\"}");
            API.SetNuiFocus(true, true);
        }

        [Command("close")]
        public void MenuClose()
        {
            API.SetNuiFocus(false, false);
            API.SendNuiMessage("{\"type\":\"close\"}");
        }
    }
}
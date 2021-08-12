using CitizenFX.Core;
using API = CitizenFX.Core.Native.API;

namespace FivePD.Gamemode.Client
{
    public class Bootstrapper : BaseScript
    {
        public Bootstrapper()
        {
            Debug.WriteLine("Hello from FivePD (Client)!");
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
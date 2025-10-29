using System.IO;
using FivePD.Server.Attributes;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server;

#pragma warning disable

[Obfuscate]
internal static class LicenseHandler
{
    public static (bool Accepted, string Error) ValidateEula()
    {
        const string defaultEulaContent =
            "By changing the below setting to TRUE, you acknowledge that you have read and understood our EULA which can be found at the following URL: {url}\n\nEULA: FALSE";

        const string eulaInstruction = "If you have read and understood the EULA, please change \"EULA: FALSE\" to \"EULA: TRUE\" and restart FivePD by typing \"restart fivepd\" in the server console.";

        var resourcePath = Cfx.API.GetResourcePath(Cfx.API.GetCurrentResourceName());
        var eulaPath = Path.Combine(resourcePath, "eula.txt");

        if (!File.Exists(eulaPath))
        {
            File.WriteAllText(eulaPath, defaultEulaContent);
            return (false, $"Could not find \"eula.txt\", so a default version has been created. {eulaInstruction}");
        }

        var content = File.ReadAllText(eulaPath).ToUpper();
        if (content.Contains("EULA: TRUE"))
        {
            return (true, string.Empty);
        }

        if (string.IsNullOrEmpty(content) || !content.Contains("EULA: FALSE"))
        {
            File.WriteAllText(eulaPath, defaultEulaContent);
            return (false, $"The \"eula.txt\" was malformed, so it has been fixed. {eulaInstruction}");
        }

        return (false, eulaInstruction);
    }
}
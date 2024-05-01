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

        var resourcePath = Cfx.API.GetResourcePath(Cfx.API.GetCurrentResourceName());
        var eulaPath = Path.Combine(resourcePath, "eula.txt");

        if (!File.Exists(eulaPath))
        {
            File.WriteAllText(eulaPath, defaultEulaContent);
            return (false, "Could not find \"eula.txt\" so it has been created. Please accept it and restart FivePD.");
        }

        var content = File.ReadAllText(eulaPath).ToUpper();
        if (content.Contains("EULA: TRUE"))
        {
            return (true, string.Empty);
        }

        if (string.IsNullOrEmpty(content) || !content.Contains("EULA: FALSE"))
        {
            File.WriteAllText(eulaPath, defaultEulaContent);
            return (false, "The \"eula.txt\" was malformed so it has been updated. Please accept it and restart FivePD.");
        }

        if (!content.Contains("EULA: TRUE"))
        {
            return (false, "Please accept the \"eula.txt\" and restart FivePD.");
        }

        return (false, string.Empty);
    }
}
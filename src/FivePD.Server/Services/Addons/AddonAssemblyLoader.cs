using System.Linq;
using System.Reflection;
using FivePD.Server.API;
using FivePD.Server.Interfaces.Addons;
using Serilog;

namespace FivePD.Server.Services.Addons;

/// <inheritdoc />
public class AddonAssemblyLoader : IAddonAssemblyLoader
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddonAssemblyLoader"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public AddonAssemblyLoader(ILogger logger)
    {
        this._logger = logger;
    }

    /// <inheritdoc />
    public Assembly LoadAssemblyFromBytes(byte[] assemblyBytes)
    {
        var asm = Assembly.Load(assemblyBytes);

        foreach (var type in asm.GetTypes().Where(x => typeof(IExtension).IsAssignableFrom(x)))
        {
            this._logger.Debug("Found class {@Type} that identified as an addon entrypoint", type.FullName);
        }

        return asm;
    }
}
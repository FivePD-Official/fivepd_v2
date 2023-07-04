using System.Reflection;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for receiving assemblies and preparing them for
/// invocation by other parts of the game mode.
/// </summary>
public interface IAddonAssemblyLoader
{
    /// <summary>
    /// Initializes an addon assembly from bytes.
    /// </summary>
    /// <param name="assemblyBytes">The assembly to load.</param>
    /// <returns>The loaded assembly.</returns>
    public Assembly LoadAssemblyFromBytes(byte[] assemblyBytes);
}
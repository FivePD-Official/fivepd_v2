using System.Threading.Tasks;

namespace FivePD.Gamemode.Client.Interfaces;

/// <summary>
/// Provides utility methods for file handling.
/// </summary>
internal interface IFileHandlerService
{
    /// <summary>
    /// Loads a file's content.
    /// If empty, returns T with its default values.
    /// Otherwise returns the data converted to the given type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the content.</typeparam>
    /// <param name="path">The path of the file.</param>
    /// <returns>The data converted to T.</returns>
    public Task<T> LoadFileAsync<T>(string path);
}
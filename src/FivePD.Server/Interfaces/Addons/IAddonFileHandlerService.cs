namespace FivePD.Server.Interfaces.Addons;

/// <summary>
/// Provides utility methods for file handling within addons.
/// </summary>
public interface IAddonFileHandlerService
{
    /// <summary>
    /// Loads the content of the given file within the current addon.
    /// </summary>
    /// <param name="path">
    /// The path of the file.
    /// Should match with any of the specified items in the "Files" property of the addon metadata file.
    /// </param>
    /// <returns>The string content of the file.</returns>
    public string LoadFile(string path);

    /// <summary>
    /// !! IT CAN ONLY DESERIALIZE FROM JSON !! TODO: and yaml!
    /// Reads the given file within the current addon.
    /// If empty, returns T with its default values.
    /// Otherwise returns the data deserialized to the given type.
    /// Note that the deserialized data will be cached. Type upon retrieval must match the type used when first deserializing.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the content.</typeparam>
    /// <param name="path">
    /// The path of the file.
    /// Should match with any of the specified items in the "Files" property of the addon metadata file.
    /// </param>
    /// <returns>The data converted to T.</returns>
    public T LoadFile<T>(string path);
}
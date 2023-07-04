using System;

namespace FivePD.Client.Attributes;

/// <summary>
/// Used to bind methods to given keys.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class KeybindAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeybindAttribute"/> class.
    /// </summary>
    /// <param name="id">Identifier of the keybind. For example: "fivepd_open_mdt".</param>
    /// <param name="key">The key's name which will be binded to the given method.</param>
    /// <param name="description">Description of the keybind that will be showed in the settings.</param>
    public KeybindAttribute(string id, string key, string description = "")
    {
        this.Id = id;
        this.Key = key;
        this.Description = description;
    }

    /// <summary>
    /// Gets identifier of the binded action.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets key's name to bind.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets key's description.
    /// </summary>
    // TODO: Change description to JSON key for translation.
    public string Description { get; }
}
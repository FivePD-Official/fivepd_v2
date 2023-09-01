using System;

namespace FivePD.Server.Utilities;

/// <summary>
/// Provides a simple way to generate unique IDs.
/// </summary>
public static class IdGenerator
{
    /// <summary>
    /// Generates a unique ID.
    /// </summary>
    /// <returns>The generated ID.</returns>
    public static string Generate()
    {
        var guid = Guid.NewGuid().ToString();
        var parts = guid.Split('-');
        return $"{parts[0]}{parts[1]}{parts[2]}";
    }
}
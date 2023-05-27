using FivePD.Common.Attributes;

namespace FivePD.Common.Utilities;

/// <summary>
/// Contains extension methods for dynamic objects.
/// </summary>
[NoUsage]
public static class DynamicExtensions
{
    /// <summary>
    /// Gets a value from the given object by its path.
    /// </summary>
    /// <param name="obj">The object to search.</param>
    /// <param name="path">The path to the value.</param>
    /// <returns>The found value.</returns>
    public static string GetValueByPath(this object obj, string path)
    {
        if (obj is null)
        {
            return string.Empty;
        }

        var split = path.Split('.');
        var currentObj = (dynamic)obj;
        foreach (var item in split)
        {
            if (currentObj[item] is null)
            {
                return string.Empty;
            }

            currentObj = currentObj[item];
        }

        return (string)currentObj;
    }
}
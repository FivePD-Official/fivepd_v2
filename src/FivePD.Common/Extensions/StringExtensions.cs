using System.Linq;

namespace FivePD.Common.Extensions;

/// <summary>
/// Holds extension methods for strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Creates an acronym from every word's first character in the given string.
    /// </summary>
    /// <param name="text">The text to create the acronym from.</param>
    /// <returns>The created acronym.</returns>
    public static string ToAcronym(this string text)
    {
        return string.Join(
                string.Empty,
                text
                    .Split()
                    .Where(s => s.Length > 0 && char.IsLetter(s[0]))
                    .Select(s => s[0])
                    .ToArray())
            .ToUpper();
    }
}
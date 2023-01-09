using System;
using System.Collections.Generic;
using System.Linq;

namespace FivePD.Common.Extensions
{
    /// <summary>
    /// Holds extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces parameter identifiers with their values depending on its template.
        /// The template {{key}} will be converted to formatting + key.
        /// The template {{key(value)}} will be converted to formatting + value.
        /// More in-depth example: https://github.com/GTAPoliceMods/FivePD/issues/46#issuecomment-1100093724.
        /// </summary>
        /// <param name="text">The text that's parameters needs to be replaced.</param>
        /// <param name="parameters">A dictionary containing the key + formatting pairs.</param>
        /// <param name="formattingReset">This string will be placed after the replaced value to reset its formatting.</param>
        /// <returns>The new string with the replaced parameters.</returns>
        public static string ReplaceParams(this string text, Dictionary<string, string> parameters, string formattingReset = "")
        {
            foreach (var item in parameters)
            {
                var startIndex = text.IndexOf("{{" + item.Key + "}}", StringComparison.Ordinal);
                if (startIndex == -1)
                {
                    startIndex = text.IndexOf("{{" + item.Key + "(", StringComparison.Ordinal);
                    for (var i = startIndex; i < text.Length; i++)
                    {
                        if (text[i] != ')')
                        {
                            continue;
                        }

                        var sub = text.Substring(startIndex, i - startIndex + 3);
                        if (!sub.StartsWith("{{") || !sub.EndsWith("}}"))
                        {
                            continue;
                        }

                        var newValue = text.Substring(startIndex + 2 + item.Key.Length + 1, i - startIndex - item.Key.Length - 3);
                        text = text.Replace(sub, item.Value + newValue + formattingReset);
                    }
                }
                else
                {
                    text = text.Replace("{{" + item.Key + "}}", item.Value + formattingReset);
                }
            }

            return text;
        }

        /// <summary>
        /// Creates an acronym from every word's first character
        /// in the passed string.
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
                    .ToArray());
        }
    }
}
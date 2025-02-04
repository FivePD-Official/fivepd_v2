using FivePD.Common.Enums;
using FivePD.Common.Utilities;

namespace FivePD.Common.Extensions;

/// <summary>
/// Contains extension methods for <see cref="FPriority"/>.
/// </summary>
public static class FPriorityExtensions
{
    /// <summary>
    /// Returns the i18 label key for the given priority.
    /// </summary>
    /// <param name="priority">The priority.</param>
    /// <returns>The i18n key of the priority label.</returns>
    public static string GetLabelKey(this FPriority priority)
    {
        return priority switch
        {
            FPriority.Code1 => "LABEL.PRIORITY.CODE1",
            FPriority.Code2 => "LABEL.PRIORITY.CODE2",
            FPriority.Code3 => "LABEL.PRIORITY.CODE3",
            FPriority.Code99 => "LABEL.PRIORITY.CODE99",
            _ => string.Empty,
        };
    }
}
using System;

namespace FivePD.Server.Utilities;

/// <summary>
/// Provides helper methods for mathematical operations.
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Clamps the specified value within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the value to clamp.</typeparam>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value in the range.</param>
    /// <param name="max">The maximum value in the range.</param>
    /// <returns>
    /// If <paramref name="value"/> is less than <paramref name="min"/>, returns <paramref name="min"/>.
    /// If <paramref name="value"/> is greater than <paramref name="max"/>, returns <paramref name="max"/>.
    /// Otherwise, returns <paramref name="value"/>.
    /// </returns>
    /// <remarks>
    /// This method compares <paramref name="value"/> to <paramref name="min"/> and <paramref name="max"/> and returns
    /// <paramref name="min"/> if <paramref name="value"/> is less than <paramref name="min"/>, <paramref name="max"/> if
    /// <paramref name="value"/> is greater than <paramref name="max"/>, and <paramref name="value"/> otherwise.
    /// </remarks>
    public static T Clamp<T>(T value, T min, T max)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
        {
            return min;
        }
        else if (value.CompareTo(max) > 0)
        {
            return max;
        }
        else
        {
            return value;
        }
    }
}
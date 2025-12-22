namespace FivePD.Common;

#pragma warning disable

/// <summary>
/// Represents squared distance values for performance optimization.
/// </summary>
internal readonly record struct SquaredDistances
{
    public const float _5 = 5f * 5f;

    public const float _10 = 10f * 10f;

    public const float _15 = 15f * 15f;

    public const float _50 = 50f * 50f;

    public const float _100 = 100f * 100f;

    public const float _200 = 200f * 200f;
}

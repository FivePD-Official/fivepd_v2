using System;

namespace FivePD.Common.Enums;

#pragma warning disable

[Flags]
public enum FAnimationFlags
{
    None = 0,
    Loop = 1,
    StayInEndFrame = 2,
    UpperBodyOnly = 16, // 0x00000010
    AllowRotation = 32, // 0x00000020
    CancelableWithMovement = 128, // 0x00000080
    RagdollOnCollision = 4194304,
}
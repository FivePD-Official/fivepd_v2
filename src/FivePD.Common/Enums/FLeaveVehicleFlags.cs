namespace FivePD.Common.Enums;

#pragma warning disable

public enum FLeaveVehicleFlags
{
    None = 0,
    WarpOut = 16, // 0x00000010
    LeaveDoorOpen = 256, // 0x00000100
    BailOut = 4096,
}
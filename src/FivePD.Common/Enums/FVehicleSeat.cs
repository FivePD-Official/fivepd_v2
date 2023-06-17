namespace FivePD.Gamemode.Common.Enums;

#pragma warning disable

public enum FVehicleSeat
{
    None = -3, // 0xFFFFFFFD
    Any = -2, // 0xFFFFFFFE
    Driver = -1, // 0xFFFFFFFF
    LeftFront = -1, // 0xFFFFFFFF
    Passenger = 0,
    RightFront = 0,
    LeftRear = 1,
    RightRear = 2,
    ExtraSeat1 = 3,
    ExtraSeat2 = 4,
    ExtraSeat3 = 5,
    ExtraSeat4 = 6,
    ExtraSeat5 = 7,
    ExtraSeat6 = 8,
    ExtraSeat7 = 9,
    ExtraSeat8 = 10, // 0x0000000A
    ExtraSeat9 = 11, // 0x0000000B
    ExtraSeat10 = 12, // 0x0000000C
    ExtraSeat11 = 13, // 0x0000000D
    ExtraSeat12 = 14, // 0x0000000E
}
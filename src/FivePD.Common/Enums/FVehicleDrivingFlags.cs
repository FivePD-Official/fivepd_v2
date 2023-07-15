using System;

namespace FivePD.Common.Enums;

#pragma warning disable

[Flags]
public enum FVehicleDrivingFlags : uint
{
    None = 0,
    FollowTraffic = 1,
    YieldToPeds = 2,
    AvoidVehicles = 4,
    AvoidEmptyVehicles = 8,
    AvoidPeds = 16, // 0x00000010
    AvoidObjects = 32, // 0x00000020
    StopAtTrafficLights = 128, // 0x00000080
    UseBlinkers = 256, // 0x00000100
    AllowGoingWrongWay = 512, // 0x00000200
    Reverse = 1024, // 0x00000400
    AllowMedianCrossing = 262144, // 0x00040000
    DriveBySight = 4194304, // 0x00400000
    IgnorePathFinding = 16777216, // 0x01000000
    TryToAvoidHighways = 536870912, // 0x20000000
    StopAtDestination = 2147483648, // 0x80000000
}
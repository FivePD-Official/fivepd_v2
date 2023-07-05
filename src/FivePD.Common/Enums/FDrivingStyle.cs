namespace FivePD.Common.Enums;

#pragma warning disable

public enum FDrivingStyle
{
    None = 0,
    StopBeforeVehicles = 1,
    StopBeforePeds = 2,
    AvoidVehicles = 4,
    AvoidEmptyVehicles = 8,
    AvoidPeds = 16,
    AvoidObjects = 32,
    StopAtTrafficLights = 128,
    AllowWrongWay = 512,
    DriveInReverse = 1024,
    TakeShortestPath = 262144,
    AllowOvertake = 524288,
    IgnoreRoads = 4194304,
    IgnorePath = 16777216,

    Normal = StopBeforeVehicles | StopBeforePeds | AvoidObjects | AvoidPeds | StopAtTrafficLights,
    Emergency = TakeShortestPath | AllowOvertake | AllowWrongWay | AvoidVehicles | AvoidEmptyVehicles | AvoidObjects | AvoidPeds,
    Flee =  StopBeforeVehicles | StopBeforePeds | AvoidVehicles | AvoidEmptyVehicles | AvoidObjects | AvoidPeds | TakeShortestPath | AllowOvertake | AllowWrongWay | IgnoreRoads | IgnorePath
}
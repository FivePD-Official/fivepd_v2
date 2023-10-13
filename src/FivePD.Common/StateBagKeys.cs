namespace FivePD.Common;

#pragma warning disable

internal readonly record struct StateBagKeys
{
    public const string DutyStatus = "FivePD:DutyStatus";

    public const string Availability = "FivePD:Availability";

    public const string Status = "FivePD:Status";

    public const string Dispatcher = "FivePD:Dispatcher";

    public const string UnitCallsign = "FivePD:UnitCallsign";

    public readonly record struct Ped
    {
        public const string Stopped = "FivePD:Ped:Stopped";

        public const string HasGeneratedData = "FivePD:Ped:HasGeneratedData";

        public const string Detained = "FivePD:Ped:Detained";

        public const string Movement = "FivePD:Ped:Movement";
    }

    public readonly record struct Vehicle
    {
        public const string Stopped = "FivePD:Vehicle:Stopped";

        public const string HasGeneratedData = "FivePD:Vehicle:HasGeneratedData";
    }
}
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Common.Models;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

internal class TaskService : ITaskService
{
    public int EnterVehicle(Ped ped, Vehicle vehicle, VehicleSeat preferredSeat = VehicleSeat.Any, int timeout = -1, float speed = 1.0f, int flag = 1)
    {
        if (ped is null || vehicle is null)
        {
            return -1;
        }

        var preferredPassengerSeats = new List<VehicleSeat>
        {
            VehicleSeat.LeftRear,
            VehicleSeat.RightRear,
            VehicleSeat.ExtraSeat1,
            VehicleSeat.ExtraSeat2,
            VehicleSeat.ExtraSeat3,
            VehicleSeat.ExtraSeat4,
            VehicleSeat.ExtraSeat5,
            VehicleSeat.ExtraSeat6,
            VehicleSeat.ExtraSeat7,
            VehicleSeat.ExtraSeat8,
            VehicleSeat.ExtraSeat9,
            VehicleSeat.ExtraSeat10,
            VehicleSeat.ExtraSeat11,
            VehicleSeat.ExtraSeat12,
        };

        foreach (var s in preferredPassengerSeats)
        {
            if (vehicle.IsSeatFree(s))
            {
                preferredSeat = s;
                break;
            }
        }

        if (preferredSeat == VehicleSeat.Any && vehicle.IsSeatFree(VehicleSeat.Passenger))
        {
            preferredSeat = VehicleSeat.Passenger;
        }

        if (preferredSeat != VehicleSeat.Any)
        {
            ped.Task.EnterVehicle(vehicle, preferredSeat);
            return 1;
        }

        return 0;
    }
}
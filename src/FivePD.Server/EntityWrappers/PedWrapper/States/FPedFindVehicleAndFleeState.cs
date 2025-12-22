using System;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedFindVehicleAndFleeState : FPedState
{
    private readonly IEntityService _entityService;

    public FPedFindVehicleAndFleeState(FPed fPed)
        : base(fPed)
    {
        this._entityService = ServiceLocator.GetService<IEntityService>();
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedFindVehicleAndFleeState.");
        this.FPed.Stopped.Value = false;

        var pedPosition = this.FPed.GetUnderlyingPed.Position;
        var vehicles = this._entityService.GetAllVehicles();
        var closestVehicle = vehicles
            .Where(v => Cfx.API.DoesEntityExist(v.Handle))
            .OrderBy(v => v.Position.DistanceToSquared(pedPosition))
            .FirstOrDefault();

        if (closestVehicle is not null && Cfx.API.DoesEntityExist(closestVehicle.Handle))
        {
            Cfx.API.SetEntityDistanceCullingRadius(closestVehicle.Handle, 10000f);
            this.FleeWithoutTaskSequence(closestVehicle);
        }
        else
        {
            this.FPed.Task.FleeFrom(pedPosition);
        }
        
        return Task.CompletedTask;
    }

    // private void FleeWithTaskSequence(Vehicle closestVehicle)
    // {
    //     var pedPosition = this.FPed.GetUnderlyingPed.Position;
    //     var randomOffset = new Vector3(
    //         (float)(this._randomService.Random.NextDouble() * 2 - 1) * 800f,
    //         (float)(this._randomService.Random.NextDouble() * 2 - 1) * 800f,
    //         0f);
    //
    //     var targetPos = pedPosition + randomOffset;
    //     Vector3 roadway = Vector3.Zero;
    //     Cfx.API.GetNthClosestVehicleNode(targetPos.X, targetPos.Y, targetPos.Z, 1, ref roadway, 0, 0, 0);
    //
    //     var sequence = new TaskSequence();
    //     sequence.AddTask.EnterVehicle(closestVehicle, VehicleSeat.Driver, -1, 20f, 0);
    //     sequence.AddTask.DriveTo(closestVehicle, roadway, 10f, 60f, (int)FDrivingStyle.Flee);
    //     sequence.Close();
    //
    //     // PerformSequence is not implemented yet in FPed.Task
    //     // this.FPed.Task.PerformSequence(sequence);
    //     sequence.Dispose();
    // }

    private void FleeWithoutTaskSequence(Vehicle closestVehicle)
    {
        this.FPed.Task.EnterVehicle(closestVehicle, FVehicleSeat.Driver, -1, 20f, 0);
        this.FPed.Task.CruiseWithVehicle(closestVehicle.NetworkId, 60f, (int)FDrivingStyle.Flee);
    }

    public override Task Exit()
    {
        return Task.CompletedTask;
    }
}
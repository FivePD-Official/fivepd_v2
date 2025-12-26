using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.PedWrapper.States;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using Serilog;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class PedBehaviorService : IPedBehaviorService
{
    private readonly ICfxService _cfxService;
    private readonly IRandomService _randomService;
    private readonly IProbabilityService _probabilityService;
    private readonly IConfigService _configService;
    private readonly ILogger _logger;

    private readonly Dictionary<int, FPed> _peds = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PedBehaviorService"/> class.
    /// </summary>
    /// <param name="cfxService">The CFX service for native API calls.</param>
    /// <param name="randomService">The random service for chance-based decisions.</param>
    /// <param name="probabilityService">The probability service for outcome selection.</param>
    /// <param name="configService">The configuration service to retrieve behavior settings.</param>
    /// <param name="logger">The logger for error reporting.</param>
    public PedBehaviorService(
        ICfxService cfxService,
        IRandomService randomService,
        IProbabilityService probabilityService,
        IConfigService configService,
        ILogger logger)
    {
        this._cfxService = cfxService;
        this._randomService = randomService;
        this._probabilityService = probabilityService;
        this._configService = configService;
        this._logger = logger;
    }

    /// <inheritdoc />
    public void StartBehavior(FPlayer player, FPed fPed)
    {
        if (this._peds.ContainsKey(fPed.NetworkId))
        {
            return;
        }

        this._peds.Add(fPed.NetworkId, fPed);

        fPed.Properties.SetAlwaysKeepTask(true);
        fPed.Properties.SetBlockPermanentEvents(true);
        Cfx.API.SetEntityDistanceCullingRadius(fPed.GetUnderlyingPed.Handle, 10000f);

        _ = this.ThinkAsync(player, fPed);
    }

    /// <inheritdoc />
    public void StopBehavior(FPed fPed)
    {
        if (!this._peds.ContainsKey(fPed.NetworkId))
        {
            return;
        }

        fPed.ChangeState(null);

#pragma warning disable
        // ped.Properties.SetAlwaysKeepTask(false);
        // ped.Properties.SetBlockPermanentEvents(false);
        // Cfx.API.SetEntityDistanceCullingRadius(ped.GetUnderlyingPed.Handle, 500f);
#pragma warning restore

        this.StopBehavior(fPed.NetworkId);
    }

    /// <inheritdoc />
    public void StopBehavior(int networkId)
    {
        this._peds.Remove(networkId);
    }

    /// <inheritdoc />
    public void MakeCompliant(FPed fPed)
    {
        this._cfxService.SetPedFleeAttributes(fPed.NetworkId, 0, false);
        this._cfxService.SetPedCombatAttributes(fPed.NetworkId, 46, true);
    }

    /// <inheritdoc />
    public void MakeNonCompliant(FPed fPed)
    {
        this._cfxService.SetPedFleeAttributes(fPed.NetworkId, 0, true);
        this._cfxService.SetPedCombatAttributes(fPed.NetworkId, 46, false);
    }

    /// <summary>
    /// The main decision-making loop for a ped's behavior.
    /// </summary>
    /// <param name="player">The player context for the behavior.</param>
    /// <param name="ped">The ped that is thinking.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ThinkAsync(FPlayer player, FPed ped)
    {
        while (ped.GetUnderlyingPed is not null && !ped.Properties.IsDead && this._peds.ContainsKey(ped.NetworkId))
        {
            try
            {
                if (ped.Properties.IsInVehicle)
                {
                    var vehicleHandle = Cfx.API.GetVehiclePedIsIn(ped.GetUnderlyingPed.Handle, false);
                    if (vehicleHandle == 0)
                    {
                        return;
                    }

                    var fVehicle = FVehicle.FromVehicle(new Vehicle(vehicleHandle));
                    if (fVehicle is not null)
                    {
                        Cfx.API.SetEntityDistanceCullingRadius(fVehicle.GetUnderlyingVehicle.Handle, 10000f);

                        var config = this._configService.GeneralConfig.Peds.Behavior.Chances.InVehicle;

                        var probabilities = new List<(int Probability, Action Outcome)>
                        {
                            (config.DriveVehiclePullOver, () => ped.ChangeState(new FPedDriveVehiclePullOverState(ped, fVehicle))),
                            (config.LeaveVehicle, () => ped.ChangeState(new FPedLeaveVehicleState(ped, fVehicle))),
                            (config.DriveVehicleFlee, () => ped.ChangeState(new FPedDriveVehicleFleeState(ped, fVehicle))),
                        };

                        var selectedAction = this._probabilityService.SelectOutcomeBasedOnCumulativeChance(probabilities);
                        selectedAction?.Invoke();
                    }
                }
                else
                {
                    var config = this._configService.GeneralConfig.Peds.Behavior.Chances.OnFoot;

                    var onFootProbabilities = new List<(int Probability, Action Outcome)>
                    {
                        (config.StandStill, () => ped.ChangeState(new FPedStandStillState(ped))),
                        (config.Flee, () => ped.ChangeState(new FPedFleeState(ped, player.FPed))),
                        (config.FindVehicleAndFlee, () => ped.ChangeState(new FPedFindVehicleAndFleeState(ped))),
                        (config.ShootAtTargets, () => ped.ChangeState(new FPedShootAtTargetsState(
                            ped,
                            new List<FPed> { player.FPed }))),
                    };

                    var selectedOnFootAction = this._probabilityService.SelectOutcomeBasedOnCumulativeChance(onFootProbabilities);
                    selectedOnFootAction?.Invoke();
                }
            }
            catch (Exception exception)
            {
                this._logger.Warning($"Failed to handle ped behavior for {ped.GetUnderlyingPed.Handle}: {exception.Message}");
            }

            var behaviorConfig = this._configService.GeneralConfig.Peds.Behavior;
            var thinkTime = this._randomService.Next(behaviorConfig.MinIntervalInMs, behaviorConfig.MaxIntervalInMs);
            await Task.Delay(thinkTime);
        }

        this.StopBehavior(ped);
    }
}
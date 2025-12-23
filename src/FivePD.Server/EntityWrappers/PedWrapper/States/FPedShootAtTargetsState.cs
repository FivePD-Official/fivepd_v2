using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.Interfaces;

namespace FivePD.Server.EntityWrappers.PedWrapper.States;

#pragma warning disable

public class FPedShootAtTargetsState : FPedState
{
    private readonly List<FPed> _targets;
    private readonly IRandomService _randomService;
    private CancellationTokenSource _cts;

    public FPedShootAtTargetsState(FPed fPed, List<FPed> targets)
        : base(fPed)
    {
        this._targets = targets;
        this._randomService = ServiceLocator.GetService<IRandomService>();
    }

    public override Task Enter()
    {
        Debug.WriteLine($"Ped with network id of {this.FPed.NetworkId} entering FPedShootAtTargetsState.");
        this.FPed.Stopped.Value = false;
        this.FPed.Weapons.Give(FWeaponHash.Pistol, 100, true, true);
        this._cts = new CancellationTokenSource();
        this.RunLoopAsync(this._cts.Token);
        return Task.CompletedTask;
    }

    public override Task Exit()
    {
        this._cts?.Cancel();
        this._cts?.Dispose();
        return Task.CompletedTask;
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var ped = this.FPed.GetUnderlyingPed;
            if (ped is null)
            {
                break;
            }

            var nearbyTargets = this._targets
                .Where(target =>
                {
                    var targetPed = target.GetUnderlyingPed;
                    return targetPed is not null && ped.Position.DistanceToSquared(targetPed.Position) <= SquaredDistances._200;
                })
                .ToList();

            if (nearbyTargets.Count > 0)
            {
                var target = nearbyTargets[this._randomService.Next(0, nearbyTargets.Count)];
                this.FPed.Task.FightAgainst(target.GetUnderlyingPed, -1);
            }

            try
            {
                await Task.Delay(this._randomService.Next(3000, 7000), token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}
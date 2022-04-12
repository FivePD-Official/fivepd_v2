// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Models;
using FivePD.Gamemode.Client.Attributes;
using FivePD.Gamemode.Client.Extensions;
using FivePD.Gamemode.Client.Interfaces;
using FivePD.Gamemode.Client.Utilities;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class PedService : IPedService
    {
        /// <inheritdoc />
        [HoldableKeybind("fivepd_stop_ped", "E", "Stop ped")]
        public void StopPed()
        {
            var ped = Game.Player.GetTargetedEntity() as Ped;
            var hasPlayerAimed = false;
            if (ped is null)
            {
                ped = this.FindClosestAlivePed(Game.PlayerPed, 5f);
                if (ped is null)
                {
                    return;
                }
            }
            else
            {
                if (!ped.IsInRangeOf(Game.PlayerPed.Position, 20f) || !ped.HasHumanHash())
                {
                    return;
                }

                hasPlayerAimed = true;
            }

            this.GeneratePed(ped);

            ped.KeepTask(true);
            ped.CreateBlip(BlipSprite.Friend, FBlipColor.LightYellow, "Stopped ped");
            ped.SetRelationShip(Relationship.Companion);
            Cfx.API.DoesEntityBelongToThisScript(ped.Handle, true); // TODO: Is this needed?

            // TODO: Create a custom TaskSequence wrapper as an extension.
            ped.Task.ClearAllImmediately();
            TaskSequence ts = new TaskSequence();
            ts.AddTask.TurnTo(Game.PlayerPed);
            ts.AddTask.StandStill(-1);
            ts.Close();
            ped.Task.PerformSequence(ts);

            if (hasPlayerAimed)
            {
                // TODO: If the ped is in a vehicle exit and hands up.
            }
        }

        /// <inheritdoc />
        public void GeneratePed(Ped ped)
        {
            var fPedBuilder = new FPedBuilder(ped.NetworkId)
                .WithGender((FGender)ped.Gender);
            BaseScript.TriggerServerEvent(Events.Ped.Generate, JsonConvert.SerializeObject(fPedBuilder));
        }

        private Ped FindClosestAlivePed(Entity entity, float withinRange = 0)
        {
            var peds = World
                .GetAllPeds()
                .Where(ped => ped.HasHumanHash())
                .Where(ped => !ped.IsPlayer)
                .Where(ped => !ped.IsDead)
                .Where(ped => !ped.IsInVehicle());

            if (withinRange != 0)
            {
                peds = peds.Where(ped => ped.IsInRangeOf(entity.Position, withinRange));
            }

            return peds
                .OrderBy(ped => World.GetDistance(ped.Position, entity.Position))
                .FirstOrDefault();
        }
    }
}
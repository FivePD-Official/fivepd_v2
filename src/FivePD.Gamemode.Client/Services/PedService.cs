using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Models;
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
        private readonly EventHandlerDictionary _eventHandlerDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="PedService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
        public PedService(EventHandlerDictionary eventHandlerDictionary)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
        }

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this._eventHandlerDictionary.AddUIToServerEvent(Events.Ped.Search);
        }

        /// <inheritdoc />
        public void StopPed(Ped ped, bool handsUp = false)
        {
            this.GeneratePed(ped);

            ped.BlockPermanentEvents = true;

            if (handsUp)
            {
                var sequence = new TaskSequence();
                sequence.AddTask.HandsUp(-1);
                sequence.Close();
                ped.Task.PerformSequence(sequence);
            }

            ped.PlayAmbientSpeech("GENERIC_SHOCKED_MED");

            ped.CreateBlip(BlipSprite.Friend, FBlipColor.LightYellow, "Stopped ped");
            ped.SetRelationShip(Relationship.Companion);
        }

        /// <inheritdoc />
        public void PedLeaveVehicleWithHandsUp(Ped ped)
        {
            ped.BlockPermanentEvents = true;

            var sequence = new TaskSequence();
            sequence.AddTask.LeaveVehicle(ped.CurrentVehicle, false);
            sequence.AddTask.HandsUp(-1);
            sequence.Close();
            ped.Task.PerformSequence(sequence);

            ped.PlayAmbientSpeech("GENERIC_SHOCKED_MED");
        }

        /// <inheritdoc />
        public void GeneratePed(Ped ped)
        {
            var fPedBuilder = new FPedBuilder(ped.NetworkId)
                .WithGender((FGender)ped.Gender);
            BaseScript.TriggerServerEvent(Events.Ped.Generate, JsonConvert.SerializeObject(fPedBuilder));
        }
    }
}
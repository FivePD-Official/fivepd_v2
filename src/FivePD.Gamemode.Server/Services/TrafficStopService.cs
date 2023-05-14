using System;
using System.Collections.Concurrent;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IServiceWithEvents" />
    public class TrafficStopService : ITrafficStopService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly ConcurrentDictionary<string, TrafficStop> _trafficStops = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TrafficStopService"/> class.
        /// </summary>
        /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
        public TrafficStopService(EventHandlerDictionary eventHandlerDictionary)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
        }

        /// <inheritdoc/>
        public void RegisterEvents()
        {
            this._eventHandlerDictionary[Events.TrafficStop.Initiate] += new Action<string>(this.Initiate);
            this._eventHandlerDictionary[Events.TrafficStop.Cancel] += new Action<string>(this.Cancel);
            this._eventHandlerDictionary[Events.TrafficStop.IsVehicleAttached] += new Action<int, NetworkCallbackDelegate>(this.IsVehicleAttached);
        }

        private void Initiate(string deserializedTrafficStop)
        {
            var trafficStop = JsonConvert.DeserializeObject<TrafficStop>(deserializedTrafficStop);
            if (trafficStop is null)
            {
                return;
            }

            this._trafficStops.TryAdd(trafficStop.Id, trafficStop);
        }

        private void Cancel(string stringGuid)
        {
            this._trafficStops.TryRemove(stringGuid, out _);

            // TODO: detach attached players, etc
        }

        private void IsVehicleAttached(int vehicleNetworkId, NetworkCallbackDelegate networkCallbackDelegate)
        {
            networkCallbackDelegate.Invoke(this._trafficStops.Any(trafficStop => trafficStop.Value.StoppedVehicleNetworkId == vehicleNetworkId));
        }
    }
}
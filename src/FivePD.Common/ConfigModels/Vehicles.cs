using System.Collections.Generic;

namespace FivePD.Common.ConfigModels
{
    /// <summary>
    /// Contains the definition of the vehicles config file.
    /// </summary>
    public class Vehicles
    {
        /// <summary>
        /// Gets or sets police vehicles that can spawned with the duty menu.
        /// </summary>
        public List<PoliceVehicle> Police { get; set; } = new();

        /// <summary>
        /// Gets or sets the vehicles that'll be used for the ambulance service.
        /// </summary>
        public List<ServiceVehicle> Ambulance { get; set; } = new();

        /// <summary>
        /// Defines a police vehicle's properties.
        /// </summary>
        public class PoliceVehicle : ServiceVehicle
        {
            /// <summary>
            /// Gets or sets the name of this vehicle.
            /// </summary>
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// Defines a service vehicle's properties.
        /// </summary>
        public class ServiceVehicle
        {
            /// <summary>
            /// Gets or sets the model name of this vehicle.
            /// </summary>
            public string Model { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the livery of this vehicle.
            /// </summary>
            public int Livery { get; set; } = 0;

            /// <summary>
            /// Gets or sets the extras of this vehicle.
            /// </summary>
            public List<string> Extras { get; set; } = new();
        }
    }
}
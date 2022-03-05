// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

namespace FivePD.Common
{
    /**
     * <summary>Contains constant definitions for event handler names.</summary>
     */
    public abstract class Events
    {
        // Follows naming convention "{resource}:{area}:{id}" OR "{resource}:{area}:{id}:{type}" where,
        // {resource} is "fivepd"
        // {area} is a camelCased descriptor of the applicable event location
        // {id} is a camelCased descriptor of the event itself
        // {type} is optional, either "server" or "client"

        /**
         * <summary>Contains definitions for events related to entity management.</summary>
         */
        public abstract class EntityManagement
        {
            /**
             * <summary>C->S: Used when a client wants to request that the server creates a ped.</summary>
             */
            public const string RequestPed = "fivepd:entityManagement:requestPed";

            /**
             * <summary>C->S: Used when a client wants to request that the server creates a ped in an existing vehicle.</summary>
             */
            public const string RequestPedInVehicle = "fivepd:entityManagement:requestPedInVehicle";

            /**
             * <summary>C->S: Used when a client wants to request that the server creates a vehicle.</summary>
             */
            public const string RequestVehicle = "fivepd:entityManagement:requestVehicle";

            /**
             * <summary>C->S: Used when a client wants to request that the server creates a prop (object).</summary>
             */
            public const string RequestProp = "fivepd:entityManagement:requestProp";
        }

        /// <summary>
        /// Contains definitions for events related to localization.
        /// </summary>
        public abstract class Localization
        {
            /// <summary>
            /// C->S: Used when a client wants to request all available locales.
            /// </summary>
            public const string GetAvailableLocalesFromServer = "fivepd:localization:getAllAvaibleLocalesFromServer";

            /// <summary>
            /// Used when the Nui requests all available locales.
            /// </summary>
            public const string GetAvailableLocalesFromNui = "fivepd:nui:getAvailableLocalesFromNui";

            /// <summary>
            /// Used to request localization from Nui.
            /// </summary>
            public const string GetLocalization = "fivepd:nui:getLocalization";

            /// <summary>
            /// Changes the current localization.
            /// </summary>
            public const string ChangeLocalization = "fivepd:nui:changeLocalization";
        }

        /**
         * <summary>Contains definitions for events related to the NUI.</summary>
         */
        public abstract class Nui
        {
            /// <summary>
            /// Toggles the player's duty state.
            /// </summary>
            /// public const string ToggleDuty = "fivepd:nui:toggleDuty";

            /// <summary>
            /// Adds the selected loadout's weapons to the player.
            /// </summary>
            public const string GetLoadout = "fivepd:nui:getLoadout";
        }
    }
}
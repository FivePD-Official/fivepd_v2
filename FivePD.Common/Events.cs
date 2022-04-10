// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Common.Models;

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
        /// Contains definitions for <see cref="FPed"/> related to events.
        /// </summary>
        public abstract class Ped
        {
            /// <summary>
            /// Used to generate a <see cref="FPed"/> on the server.
            /// </summary>
            public const string Generate = "fivepd:ped:generate";
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
            /// Used to request localization from Nui.
            /// </summary>
            public const string GetLocalization = "fivepd:nui:getLocalization";

            /// <summary>
            /// Changes the current localization.
            /// </summary>
            public const string ChangeLocalization = "fivepd:nui:changeLocalization";
        }

        /// <summary>
        /// The necessary event types used for communicating with the Nui.
        /// Should contains the same types as the Terminal project has.
        /// </summary>
        public abstract class NuiEventType
        {
            /// <summary>Closes the Nui.</summary>
            public const string Close = "close";

            /// <summary>Opens the Nui and displays the MDT view.</summary>
            public const string Mdt = "view:mdt";

            /// <summary>Opens the Nui and displays a menu.</summary>
            public const string MenuView = "view:menu";

            /// <summary>Opens the Nui and displays a radial menu.</summary>
            public const string RadialMenu = "view:radial_menu";

            /// <summary>Moves up by one item in normal menus.</summary>
            public const string MenuUp = "menu:control_up";

            /// <summary>Moves down by one item in normal menus.</summary>
            public const string MenuDown = "menu:control_down";

            /// <summary>Moves left by one item in normal menus.</summary>
            public const string MenuLeft = "menu:control_left";

            /// <summary>Moves right by one item in normal menus.</summary>
            public const string MenuRight = "menu:control_right";

            /// <summary>Selects the current item.</summary>
            public const string MenuSelect = "menu:control_select";

            /// <summary>Sends the current localization to the Nui.</summary>
            public const string LocalizationChange = "localization:change";

            /// <summary>Sends all menus to the Nui.</summary>
            public const string SendMenuObjects = "send:menus";

            /// <summary>Sends all radial menus to the Nui.</summary>
            public const string SendRadialMenuObjects = "send:radialMenus";
        }
    }
}
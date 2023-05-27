using FivePD.Common.Models;

#pragma warning disable

namespace FivePD.Common
{
    // Follows naming convention "{resource}:{area}:{id}" OR "{resource}:{area}:{id}:{type}" where,
    // {resource} is "fivepd"
    // {area} is a camelCased descriptor of the applicable event location
    // {id} is a camelCased descriptor of the event itself
    // {type} is optional, either "server" or "client"
    public abstract class Events
    {
        public abstract class EntityManagement
        {
            public const string RequestPed = "fivepd:entityManagement:requestPed";

            public const string RequestPedInVehicle = "fivepd:entityManagement:requestPedInVehicle";

            public const string RequestVehicle = "fivepd:entityManagement:requestVehicle";

            public const string RequestProp = "fivepd:entityManagement:requestProp";
        }

        public abstract class Ped
        {
            public const string Generate = "fivepd:ped:generate";

            public const string Search = "fivepd:ped:search";
            
            public const string GetProperty = "fivepd:ped:getProperty";

            public const string SetProperty = "fivepd:ped:setProperty";

            public const string CreateBlip = "fivepd:ped:createBlip";

            public const string RemoveBlips = "fivepd:ped:removeBlips";

            public const string SetTask = "fivepd:ped:setTask";
        }

        public abstract class Vehicle
        {
            public const string Generate = "fivepd:vehicle:generate";
        }

        public abstract class Localization
        {
            public const string GetLocalizationFromServer = "fivepd:localization:getLocalizationFromServer";

            public const string GetLocalizationFromNui = "fivepd:localization:getLocalizationFromNui";
        }

        public abstract class NuiEventType
        {
            public const string ClientReady = "ready";

            public const string Close = "close";

            public const string Mdt = "view:mdt";

            public const string MenuView = "view:menu";

            public const string RadialMenu = "view:radial_menu";

            public const string TimerBar = "view:timer_bar";

            public const string MenuUp = "menu:control_up";

            public const string MenuDown = "menu:control_down";

            public const string MenuLeft = "menu:control_left";

            public const string MenuRight = "menu:control_right";

            public const string MenuSelect = "menu:control_select";

            public const string RadialMenuControl = "radial_menu:control";

            public const string PlaySound = "sound:play";

            public const string Speak = "sound:speak";
        }

        public abstract class TrafficStop
        {
            public const string Initiate = "fivepd:trafficStop:initiate";

            public const string Cancel = "fivepd:trafficStop:cancel";

            public const string IsVehicleAttached = "fivepd:trafficStop:isVehicleAttached";
        }

        public abstract class UI
        {
            public abstract class Player
            {
                public const string GetDetails = "fivepd:player:getDetails";

                public const string ChangeDivision = "fivepd:player:changeDivision";

                public const string ChangeRank = "fivepd:player:changeRank";

                public const string LocalizationChange = "fivepd:player:localizationChange";

                public const string GetAllPageable = "fivepd:player:getAllPageable";

                public const string ChangeRole = "fivepd:player:changeRole";
            }

            public abstract class Department
            {
                public const string List = "fivepd:department:list";

                public const string GetById = "fivepd:department:getById";

                public const string GetUsers = "fivepd:department:getUsers";

                public const string GetRank = "fivepd:department:getRank";

                public const string GetRanks = "fivepd:department:getRanks";

                public const string GetDivision = "fivepd:department:getDivision";

                public const string GetDivisions = "fivepd:department:getDivisions";

                public const string Create = "fivepd:department:create";

                public const string CheckIfDepartmentNameTaken = "fivepd:department:checkIfDepartmentNameTaken";

                public const string Join = "fivepd:department:join";

                public const string Leave = "fivepd:department:leave";

                public const string Edit = "fivepd:department:edit";

                public const string Delete = "fivepd:department:delete";

                public const string DeleteDivision = "fivepd:department:deleteDivision";

                public const string EditDivision = "fivepd:department:renameDivision";

                public const string CreateDivision = "fivepd:department:createDivision";

                public const string DeleteRank = "fivepd:department:deleteRank";

                public const string EditRank = "fivepd:department:editRank";

                public const string CreateRank = "fivepd:department:createRank";
            }

            public abstract class LooseData
            {
                public const string GetSetAttribute = "fivepd:looseData:getSetAttribute";

                public const string GetSetAttributes = "fivepd:looseData:getSetAttributes";

                public const string GetSetAttributesPageable = "fivepd:looseData:getSetAttributesPageable";

                public const string AddSetAttribute = "fivepd:looseData:addSetAttribute";

                public const string EditSetAttribute = "fivepd:looseData:editSetAttribute";

                public const string DeleteSetAttribute = "fivepd:looseData:deleteSetAttribute";

                public const string GetEntity = "fivepd:looseData:getEntity";

                public const string GetEntities = "fivepd:looseData:getEntities";

                public const string AddEntity = "fivepd:looseData:addEntity";

                public const string EditEntity = "fivepd:looseData:editEntity";

                public const string DeleteEntity = "fivepd:looseData:deleteEntity";
            }

            public abstract class Menu
            {
                public const string GetAll = "fivepd:menu:getAll";

                public const string OnButtonSelect = "fivepd:menu:onButtonSelect";

                public const string OnCheckboxSelect = "fivepd:menu:onCheckboxSelect";

                public const string OnListSelect = "fivepd:menu:onListSelect";

                public const string OnListIndexChange = "fivepd:menu:onListIndexChange";

                public const string UpdateMenuItem = "fivepd:menu:update_item";
            }
        }

        public abstract class DevTools
        {
            public const string DeleteEntities = "fivepd:devtools:deleteEntities";

            public const string GetCurrentPlayerCoordinate = "fivepd:devtools:getCurrentPlayerCoordinate";
        }

        public abstract class Control
        {
            public const string GetAll = "fivepd:control:getAll";
            
            public const string Trigger = "fivepd:control:trigger";
        }

        public abstract class Interaction
        {
            public const string GetTargetedEntity = "fivepd:interaction:getTargetedEntity";
        }

        public abstract class Generic
        {
            public const string Callback = "fivepd:generic:callback";
        }
    }
}
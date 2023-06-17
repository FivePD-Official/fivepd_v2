#pragma warning disable

namespace FivePD.Gamemode.Common;

// Follows naming convention "{resource}:{area}:{id}" OR "{resource}:{area}:{id}:{type}" where,
// {resource} is "fivepd"
// {area} is a camelCased descriptor of the applicable event location
// {id} is a camelCased descriptor of the event itself
// {type} is optional, either "server" or "client"
internal readonly record struct Events
{
    public readonly record struct Player
    {
        public const string OnConnected = "fivepd:player:onConnected";
    }

    public readonly record struct Ped
    {
        public const string Generate = "fivepd:ped:generate";

        public const string Search = "fivepd:ped:search";

        public const string GetProperty = "fivepd:ped:getProperty";

        public const string SetProperty = "fivepd:ped:setProperty";

        public const string SetTask = "fivepd:ped:setTask";

        public const string InvokeMethod = "fivepd:ped:invokeMethod";

        public const string WeaponCollection = "fivepd:ped:weaponCollection";

        public const string WeaponCollectionWithReturn = "fivepd:ped:weaponCollectionWithReturn";
    }

    public readonly record struct Vehicle
    {
        public const string Generate = "fivepd:vehicle:generate";
        
        public const string GetProperty = "fivepd:vehicle:getProperty";

        public const string SetProperty = "fivepd:vehicle:setProperty";

        public const string InvokeMethod = "fivepd:vehicle:invokeMethod";
    }

    public readonly record struct Localization
    {
        public const string GetLocalizationFromServer = "fivepd:localization:getLocalizationFromServer";

        public const string GetLocalizationFromNui = "fivepd:localization:getLocalizationFromNui";
    }

    public readonly record struct NuiEventType
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

    public readonly record struct UI
    {
        public readonly record struct Player
        {
            public const string GetDetails = "fivepd:player:getDetails";

            public const string ChangeDivision = "fivepd:player:changeDivision";

            public const string ChangeRank = "fivepd:player:changeRank";

            public const string LocalizationChange = "fivepd:player:localizationChange";

            public const string GetAllPageable = "fivepd:player:getAllPageable";

            public const string ChangeRole = "fivepd:player:changeRole";
        }

        public readonly record struct Department
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

        public readonly record struct LooseData
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

        public readonly record struct Menu
        {
            public const string GetAll = "fivepd:menu:getAll";

            public const string OnButtonSelect = "fivepd:menu:onButtonSelect";

            public const string OnCheckboxSelect = "fivepd:menu:onCheckboxSelect";

            public const string OnListSelect = "fivepd:menu:onListSelect";

            public const string OnListIndexChange = "fivepd:menu:onListIndexChange";

            public const string UpdateMenuItem = "fivepd:menu:update_item";
        }

        public readonly record struct Unit
        {
            public const string GetAll = "fivepd:unit:getAll";

            public const string Join = "fivepd:unit:join";

            public const string RequestToJoin = "fivepd:unit:requestToJoin";

            public const string Leave = "fivepd:unit:leave";
        }

        public readonly record struct Config
        {
            public const string GetGeneralSettings = "fivepd:config:getGeneralSettings";
        }
        
        public readonly record struct Callout
        {
            public const string GetActive = "fivepd:ui:callout:getActive";
        }
    }

    public readonly record struct DevTools
    {
        public const string DeleteEntities = "fivepd:devtools:deleteEntities";

        public const string GetCurrentPlayerCoordinate = "fivepd:devtools:getCurrentPlayerCoordinate";
    }

    public readonly record struct Control
    {
        public const string GetAll = "fivepd:control:getAll";

        public const string Trigger = "fivepd:control:trigger";
    }

    public readonly record struct Interaction
    {
        public const string GetTargetedEntity = "fivepd:interaction:getTargetedEntity";
    }

    public readonly record struct Generic
    {
        public const string Callback = "fivepd:generic:callback";

        public const string Action = "fivepd:generic:action";
    }

    public readonly record struct Notification
    {
        public const string ShowTooltip = "fivepd:notification:showTooltip";

        public const string ShowNotification = "fivepd:notification:showNotification";

        public const string ShowNotificationWithTexture = "fivepd:notification:showNotificationWithTexture";
    }

    public readonly record struct Marker
    {
        public const string Add = "fivepd:marker:add";

        public const string Remove = "fivepd:marker:remove";
    }

    public readonly record struct Callout
    {
        public const string OnAccepted = "fivepd:callout:onAccepted";

        public const string OnPlayerEnteredInActivityZone = "fivepd:callout:onPlayerEnteredInActivityZone";

        public const string OnCompleted = "fivepd:callout:onCompleted";

        public const string GetAllActiveUI = "fivepd:callout:getAllActiveUI";
    }
}
#pragma warning disable

namespace FivePD.Common;

// Follows naming convention "{resource}:{area}:{id}" OR "{resource}:{area}:{id}:{type}" where,
// {resource} is "fivepd"
// {area} is a camelCased descriptor of the applicable event location
// {id} is a camelCased descriptor of the event itself
// {type} is optional, either "server" or "client"
internal readonly record struct Events
{
    public readonly record struct Lifecycle
    {
        public const string Stop = "fivepd:lifecycle:stop";
    }

    public readonly record struct Player
    {
        public const string OnConnected = "fivepd:player:onConnected";

        public const string UpdateUnitAvailability = "fivepd:player:updateUnitAvailability";

        public const string UpdateUnitStatus = "fivepd:player:updateUnitStatus";
    }

    public readonly record struct Ped
    {
        public const string Generate = "fivepd:ped:generate";

        public const string GetProperty = "fivepd:ped:getProperty";

        public const string SetProperty = "fivepd:ped:setProperty";

        public const string SetTask = "fivepd:ped:setTask";

        public const string InvokeMethod = "fivepd:ped:invokeMethod";

        public const string WeaponCollection = "fivepd:ped:weaponCollection";

        public const string WeaponCollectionWithReturn = "fivepd:ped:weaponCollectionWithReturn";

        public const string OnAlerted = "fivepd:ped:onAlerted";
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

    public readonly record struct UIEvent
    {
        public const string ClientReady = "fivepd:ui:ready";

        public const string Close = "fivepd:ui:close";

        public const string Mdt = "fivepd:ui:view:mdt";

        public const string Information = "fivepd:ui:view:information";

        public const string MenuView = "fivepd:ui:view:menu";

        public const string RadialMenu = "fivepd:ui:view:radialMenu";

        public const string TimerBar = "fivepd:ui:view:timer_bar";

        public const string MenuUp = "fivepd:menu:control_up";

        public const string MenuDown = "fivepd:menu:control_down";

        public const string MenuLeft = "fivepd:menu:control_left";

        public const string MenuRight = "fivepd:menu:control_right";

        public const string MenuSelect = "fivepd:menu:control_select";

        public const string RadialMenuControl = "fivepd:radialMenu:control";

        public const string PlaySound = "fivepd:sound:play";

        public const string Speak = "fivepd:sound:speak";
    }

    public readonly record struct UI
    {
        public readonly record struct Player
        {
            public const string GetDetails = "fivepd:player:getDetails";

            public const string DismissedWelcomeModal = "fivepd:player:dismissedWelcomeModal";

            public const string ChangeDivision = "fivepd:player:changeDivision";

            public const string ChangeRank = "fivepd:player:changeRank";

            public const string ChangeFixedRank = "fivepd:player:changeFixedRank";

            public const string LocalizationChange = "fivepd:player:localizationChange";

            public const string GetAllPageable = "fivepd:player:getAllPageable";

            public const string ChangeRole = "fivepd:player:changeRole";
        }

        public readonly record struct Department
        {
            public const string GetAllPageable = "fivepd:department:getAllPageable";

            public const string Get = "fivepd:department:get";

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
            public const string GetGeneralConfig = "fivepd:config:getGeneralConfig";

            public const string GetSafeZoneSize = "fivepd:config:getSafeZoneSize";
        }

        public readonly record struct Callout
        {
            public const string GetActive = "fivepd:ui:callout:getActive";

            public const string GetAllTypes = "fivepd:ui:callout:getAllTypes";

            public const string Dispatch = "fivepd:ui:callout:dispatch";
        }

        public readonly record struct Dispatch
        {
            public const string GetIncomingCallouts = "fivepd:ui:dispatch:getIncomingCallouts";

            public const string GetUnits = "fivepd:ui:dispatch:getUnits";

            public const string UpdatePlayers = "fivepd:ui:dispatch:updatePlayers";

            public const string Enable = "fivepd:ui:dispatch:enable";

            public const string Disable = "fivepd:ui:dispatch:disable";
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
    
    public readonly record struct Entity
    {
        public const string GetClosestEntityToPlayer = "fivepd:entity:getClosestEntityToPlayer";
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

        public const string SelfAssign = "fivepd:callout:selfAssign";
    }

    public readonly record struct Config
    {
        public const string Get = "fivepd:config:getGeneral";
    }

    public readonly record struct Dispatch
    {
        public const string EnableForPlayer = "fivepd:dispatch:enableForPlayer";

        public const string DisableForPlayer = "fivepd:dispatch:disableForPlayer";

        public const string SendCalloutToDispatcher = "fivepd:dispatch:sendCalloutToDispatcher";

        public const string RemoveCalloutFromDispatcher = "fivepd:dispatch:removeCalloutFromDispatcher";

        public const string DismissCallout = "fivepd:dispatch:dismissCallout";

        public const string OnUnitCreated = "fivepd:dispatch:onUnitCreated";

        public const string OnUnitRemoved = "fivepd:dispatch:onUnitRemoved";

        public const string AssignCalloutToUnits = "fivepd:dispatch:assignCalloutToUnits";
    }

    public readonly record struct IncidentHistory
    {
        public const string Get = "fivepd:incidentHistory:get";

        public const string GetAllPageable = "fivepd:incidentHistory:getAllPageable";

        public const string GetAllOfUser = "fivepd:incidentHistory:getAllOfUser";
    }

    public readonly record struct PedHistory
    {
        public const string Get = "fivepd:pedHistory:get";

        public const string GetAllPageable = "fivepd:pedHistory:getAllPageable";

        public const string GetAllSimple = "fivepd:pedHistory:getAllSimple";
    }

    public readonly record struct Cfx
    {
        // void AddAmmoToPed(int ped,uint weaponHash,int ammo);
        public const string AddAmmoToPed = "fivepd:cfx:AddAmmoToPed";

        // void AddAmmoToPedByType(int ped,int ammoType,int ammo);
        public const string AddAmmoToPedByType = "fivepd:cfx:AddAmmoToPedByType";

        // void AddArmourToPed(int ped,int amount);
        public const string AddArmourToPed = "fivepd:cfx:AddArmourToPed";

        // void AddPedAmmo(int ped,int ammoType,int ammo);;
        public const string AddPedAmmo = "fivepd:cfx:AddPedAmmo";

        // void ApplyDamageToPed(int ped,int damageAmount,bool armorFirst);;
        public const string ApplyDamageToPed = "fivepd:cfx:ApplyDamageToPed";

        // void ApplyForceToEntity(int entity,int forceType,float x,float y,float z,float offX,float offY,float offZ,int boneIndex,bool isDirectionRel,bool ignoreUpVec,bool isForceRel,bool p12,bool p13);;
        public const string ApplyForceToEntity = "fivepd:cfx:ApplyForceToEntity";

        // void ApplyForceToEntityCenterOfMass(int entity,int forceType,float x,float y,float z,bool p5,bool isDirectionRel,bool isForceRel,bool p8);;
        public const string ApplyForceToEntityCenterOfMass = "fivepd:cfx:ApplyForceToEntityCenterOfMass";

        // int  GetClosestVehicle(float x,float y,float z,float radius,uint modelHash,int flags);
        public const string GetClosestVehicle = "fivepd:cfx:GetClosestVehicle";

        // void AttachEntityToEntity(int entity1,int entity2,int boneIndex,float xPos,float yPos,float zPos,float xRot,float yRot,float zRot,bool p9,bool useSoftPinning,bool collision,bool isPed,int rotationOrder,bool fixedRot);
        public const string AttachEntityToEntity = "fivepd:cfx:AttachEntityToEntity";

        // void SetPedIntoVehicle(int ped,int vehicle,int seatIndex);
        public const string SetPedIntoVehicle = "fivepd:cfx:SetPedIntoVehicle";
    }
}
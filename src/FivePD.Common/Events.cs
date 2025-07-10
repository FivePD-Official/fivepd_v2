#pragma warning disable

namespace FivePD.Common;

// Follows naming convention "{resource}:{area}:{id}" OR "{resource}:{area}:{id}:{type}" where,
// {resource} is "fivepd"
// {area} is a camelCased descriptor of the applicable event location
// {id} is a camelCased descriptor of the event itself
// {type} is optional, either "server" or "client"
internal readonly record struct Events
{
    public readonly record struct NetworkEvent
    {
        public const string ClientToServer = "fivepd:networkEvent:clientToServer";
    }

    public readonly record struct Lifecycle
    {
        public const string ServerBootstrapped = "fivepd:lifecycle:serverBootstrapped";

        public const string Stop = "fivepd:lifecycle:stop";
    }

    public readonly record struct Feedback
    {
        public const string Send = "fivepd:feedback:send";
    }

    public readonly record struct AccessControl
    {
        public const string HasAccess = "fivepd:accessControl:hasAccess";

        public const string GiveAccess = "fivepd:accessControl:give";

        public const string RemoveAccess = "fivepd:accessControl:remove";

        public const string GiveAccessUI = "fivepd:accessControl:giveUI";

        public const string RemoveAccessUI = "fivepd:accessControl:removeUI";
    }

    public readonly record struct Vector3
    {
        public const string GetRandomOffset = "fivepd:vector3:getRandomOffset";

        public const string GetPointOnRoadSide = "fivepd:vector3:GetPointOnRoadSide";
    }

    public readonly record struct Player
    {
        public const string OnConnected = "fivepd:player:onConnected";

        public const string UpdateUnitAvailability = "fivepd:player:updateUnitAvailability";

        public const string UpdateUnitStatus = "fivepd:player:updateUnitStatus";

        public const string UpdatePanicModeState = "fivepd:player:updatePanicModeState";

        public const string GetDismissedWelcomeModal = "fivepd:player:getDismissedWelcomeModal";

        public const string DismissedWelcomeModal = "fivepd:player:dismissedWelcomeModal";

        public const string GetRole = "fivepd:player:getRole";

        public const string OnRoleChange = "fivepd:player:onRoleChange";
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

        public const string OnAlerted = "fivepd:vehicle:onAlerted";

        public const string SetCosmetics = "fivepd:vehicle:setCosmetics";

        public const string Get = "fivepd:vehicle:get";

        public const string GetAllPageable = "fivepd:vehicle:getAllPageable";
    }

    public readonly record struct ALPR
    {
        public const string Check = "fivepd:alpr:check";
    }

    public readonly record struct Localization
    {
        public const string GetDefaultOrPreferred = "fivepd:localization:getDefaultOrPreferred";

        public const string GetLocalizationFromNui = "fivepd:localization:getLocalizationFromNui";

        public const string GetAvailableLocalizations = "fivepd:localization:getAvailableLocalizations";
    }

    public readonly record struct UIEvent
    {
        public const string ClientReady = "fivepd:ui:ready";

        public const string Close = "fivepd:ui:close";

        public const string Mdt = "fivepd:ui:view:mdt";

        public const string Information = "fivepd:ui:view:information";

        public const string WelcomeModal = "fivepd:ui:view:welcomeModal";

        public const string MenuView = "fivepd:ui:view:menu";

        public const string RadialMenu = "fivepd:ui:view:radialMenu";

        public const string TimerBar = "fivepd:ui:view:timer_bar";

        public const string TimerBarProgress = "fivepd:ui:view:timer_bar_progress"; // New event

        public const string MenuUp = "fivepd:menu:control_up";

        public const string MenuDown = "fivepd:menu:control_down";

        public const string MenuLeft = "fivepd:menu:control_left";

        public const string MenuRight = "fivepd:menu:control_right";

        public const string MenuSelect = "fivepd:menu:control_select";

        public const string RadialMenuControl = "fivepd:radialMenu:control";
    }

    public readonly record struct Rank
    {
        public const string Get = "fivepd:rank:get";

        public const string GetAllGlobal = "fivepd:rank:getAllGlobal";

        public const string GetAllPageable = "fivepd:rank:getAllPageable";

        public const string Create = "fivepd:rank:create";

        public const string CreateGlobal = "fivepd:rank:createGlobal";

        public const string Edit = "fivepd:rank:edit";

        public const string EditGlobal = "fivepd:rank:editGlobal";

        public const string Delete = "fivepd:rank:delete";
    }

    public readonly record struct UI
    {
        public readonly record struct Player
        {
            public const string GetDetails = "fivepd:player:getDetails";

            public const string ChangeDivision = "fivepd:player:changeDivision";

            public const string ChangeRank = "fivepd:player:changeRank";

            public const string ChangeFixedRank = "fivepd:player:changeFixedRank";

            public const string LocalizationChange = "fivepd:player:localizationChange";

            public const string GetAllPageable = "fivepd:player:getAllPageable";

            public const string ChangeRole = "fivepd:player:changeRole";

            public const string UpdateDetails = "fivepd:player:updateDetails";
        }

        public readonly record struct Department
        {
            public const string GetAll = "fivepd:department:getAll";

            public const string GetAllPageable = "fivepd:department:getAllPageable";

            public const string Get = "fivepd:department:get";

            public const string GetUsers = "fivepd:department:getUsers";

            public const string GetDivision = "fivepd:department:getDivision";

            public const string GetDivisions = "fivepd:department:getDivisions";

            public const string Create = "fivepd:department:create";

            public const string CheckIfDepartmentNameTaken = "fivepd:department:checkIfDepartmentNameTaken";

            public const string Join = "fivepd:department:join";

            public const string Leave = "fivepd:department:leave";

            public const string Edit = "fivepd:department:edit";

            public const string EditCallsignFormat = "fivepd:department:editCallsignFormat";

            public const string Delete = "fivepd:department:delete";

            public const string DeleteDivision = "fivepd:department:deleteDivision";

            public const string EditDivision = "fivepd:department:renameDivision";

            public const string CreateDivision = "fivepd:department:createDivision";

            public const string GetAllSimpleWithArea = "fivepd:department:getAllSimpleWithArea";

            public const string UpdateAreas = "fivepd:department:updateAreas";

            public const string RemoveUser = "fivepd:department:removeUser";
        }

        public readonly record struct CalloutDetails
        {
            public const string GetAllPageable = "fivepd:calloutDetails:getAllPageable";

            public const string Get = "fivepd:calloutDetails:get";

            public const string UpdateRuleset = "fivepd:calloutDetails:updateRuleset";
        }

        public readonly record struct VariableData
        {
            public const string GetSetAttribute = "fivepd:variableData:getSetAttribute";

            public const string GetSetAttributes = "fivepd:variableData:getSetAttributes";

            public const string GetSetAttributesPageable = "fivepd:variableData:getSetAttributesPageable";

            public const string AddSetAttribute = "fivepd:variableData:addSetAttribute";

            public const string EditSetAttribute = "fivepd:variableData:editSetAttribute";

            public const string DeleteSetAttribute = "fivepd:variableData:deleteSetAttribute";

            public const string GetEntity = "fivepd:variableData:getEntity";

            public const string GetEntitiesPageable = "fivepd:variableData:getEntitiesPageable";

            public const string GetEntities = "fivepd:variableData:getEntities";

            public const string AddEntity = "fivepd:variableData:addEntity";

            public const string EditEntity = "fivepd:variableData:editEntity";

            public const string DeleteEntity = "fivepd:variableData:deleteEntity";
        }

        public readonly record struct Menu
        {
            public const string GetAll = "fivepd:menu:getAll";

            public const string OnOpen = "fivepd:menu:onMenuOpen";

            public const string OnButtonSelect = "fivepd:menu:onButtonSelect";

            public const string OnCheckboxSelect = "fivepd:menu:onCheckboxSelect";

            public const string OnListSelect = "fivepd:menu:onListSelect";

            public const string OnListIndexChange = "fivepd:menu:onListIndexChange";

            public const string UpdateMenuItem = "fivepd:menu:update_item";

            public const string UpdateMenuListOption = "fivepd:menu:update_menu_list_option";

            public const string AddMenuItem = "fivepd:menu:add_menu_item";

            public const string RemoveMenuItem = "fivepd:menu:remove_menu_item";
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
            public const string GetAllTypes = "fivepd:ui:callout:getAllTypes";

            public const string Dispatch = "fivepd:ui:callout:dispatch";
        }

        public readonly record struct Dispatch
        {
            public const string GetCallouts = "fivepd:ui:dispatch:getCallouts";

            public const string GetUnits = "fivepd:ui:dispatch:getUnits";

            public const string UpdatePlayers = "fivepd:ui:dispatch:updatePlayers";

            public const string Enable = "fivepd:ui:dispatch:enable";

            public const string Disable = "fivepd:ui:dispatch:disable";
        }

        public readonly record struct VehicleConfig
        {
            public const string Get = "fivepd:ui:vehicleConfig:get";

            public const string GetAllPageable = "fivepd:ui:vehicleConfig:getAllPageable";

            public const string Create = "fivepd:ui:vehicleConfig:create";

            public const string Delete = "fivepd:ui:vehicleConfig:delete";

            public const string Edit = "fivepd:ui:vehicleConfig:edit";
        }

        public readonly record struct Location
        {
            public const string Get = "fivepd:ui:location:get";

            public const string GetAllPageable = "fivepd:ui:location:getAllPageable";

            public const string Create = "fivepd:ui:location:create";

            public const string Delete = "fivepd:ui:location:delete";
        }

        public readonly record struct LoadoutConfig
        {
            public const string Get = "fivepd:ui:loadoutConfig:get";

            public const string GetAllPageable = "fivepd:ui:loadoutConfig:getAllPageable";

            public const string Create = "fivepd:ui:loadoutConfig:create";

            public const string Delete = "fivepd:ui:loadoutConfig:delete";

            public const string Edit = "fivepd:ui:loadoutConfig:edit";
        }

        public readonly record struct Weapon
        {
            public const string Get = "fivepd:ui:weapon:get";

            public const string GetAllPageable = "fivepd:ui:weapon:getAllPageable";

            public const string GetAllSimple = "fivepd:ui:weapon:getAllSimple";

            public const string Create = "fivepd:ui:weapon:create";

            public const string Delete = "fivepd:ui:weapon:delete";

            public const string Edit = "fivepd:ui:weapon:edit";

            public const string IsWeaponValid = "fivepd:ui:weapon:isWeaponValid";
        }

        public readonly record struct WeaponComponent
        {
            public const string Get = "fivepd:ui:weaponComponent:get";

            public const string GetAllPageable = "fivepd:ui:weaponComponent:getAllPageable";

            public const string GetAllSimple = "fivepd:ui:weaponComponent:getAllSimple";

            public const string Create = "fivepd:ui:weaponComponent:create";

            public const string Delete = "fivepd:ui:weaponComponent:delete";

            public const string Edit = "fivepd:ui:weaponComponent:edit";
        }
    }

    public readonly record struct VehicleConfig
    {
        public const string RequestPoliceVehicleModels = "fivepd:vehicleConfig:requestPoliceVehicleModels";

        public const string UpdatePoliceVehicleModels = "fivepd:vehicleConfig:updatePoliceVehicleModels";
    }

    public readonly record struct Location
    {
        public const string Update = "fivepd:location:update";
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

        public const string ToggleFollowPlayer = "fivepd:ped:toggleFollowPlayer";
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

    public readonly record struct GameNotification
    {
        public const string Notify = "fivepd:gameNotification:notify";
    }

    public readonly record struct Marker
    {
        public const string Add = "fivepd:marker:add";

        public const string Remove = "fivepd:marker:remove";
    }

    public readonly record struct Incident
    {
        public const string OnAssigned = "fivepd:incident:onAssigned";

        public const string OnCompleted = "fivepd:incident:onCompleted";

        public const string Get = "fivepd:incident:get";

        public const string GetAllActive = "fivepd:incident:getAllActive";

        public const string SelfAssign = "fivepd:incident:selfAssign";

        public const string SelfUnassign = "fivepd:incident:selfUnassign";

        public const string GetAssigned = "fivepd:incident:getAssigned";

        public const string Complete = "fivepd:incident:complete";
    }

    public readonly record struct Callout
    {
        public const string OnPlayerEnteredInActivityZone = "fivepd:callout:onPlayerEnteredInActivityZone";
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

        public const string UnassignCalloutFromUnits = "fivepd:dispatch:unassignCalloutFromUnits";

        public const string UpdateRequest = "fivepd:dispatch:updateRequest";
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

    public readonly record struct Message
    {
        public const string Get = "fivepd:message:get";

        public const string GetAllPageable = "fivepd:message:getAllPageable";

        public const string Send = "fivepd:message:send";

        public const string Read = "fivepd:message:read";
    }

    public readonly record struct User
    {
        public const string GetAllSimple = "fivepd:user:getAllSimple";
    }

    public readonly record struct SceneManagement
    {
        public const string GetAllSpeedZones = "fivepd:sceneManagement:getAllSpeedZones";

        public const string CreateSpeedZone = "fivepd:sceneManagement:createSpeedZone";

        public const string DeleteSpeedZone = "fivepd:sceneManagement:deleteSpeedZone";

        public const string DeleteUI = "fivepd:sceneManagement:deleteUI";
    }

    public readonly record struct RideShare
    {
        public const string WaypointSelected = "fivepd:rideshare:waypoint:selected::";
    }

    public readonly record struct Audio
    {
        public const string PlaySound = "fivepd:audio:playSound";

        public const string Speak = "fivepd:audio:speak";
    }

    public readonly record struct AssistanceService
    {
        public const string GetTypeConfig = "fivepd:assistanceService:getTypeConfig";

        public const string GetTypeConfigPage = "fivepd:assistanceService:getTypeConfigPage";

        public const string EditTypeConfig = "fivepd:assistanceService:editTypeConfig";

        public const string GetStation = "fivepd:assistanceService:getStation";

        public const string GetStationPage = "fivepd:assistanceService:getStationPage";

        public const string GetStationTypeConfigsPage = "fivepd:assistanceService:getStationTypeConfigsPage";

        public const string GetStationTypeConfig = "fivepd:assistanceService:getStationTypeConfig";

        public const string AddStationTypeConfig = "fivepd:assistanceService:addStationTypeConfig";

        public const string EditStationTypeConfig = "fivepd:assistanceService:editStationTypeConfig";

        public const string DeleteStationTypeConfig = "fivepd:assistanceService:deleteStationTypeConfig";

        public const string GetAvailableStationTypeConfigs = "fivepd:assistanceService:getAvailableStationTypeConfigs";

        public const string CreateStation = "fivepd:assistanceService:createStation";

        public const string DeleteStation = "fivepd:assistanceService:deleteStation";
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

        // void DetachEntity(int entity, bool _dynamic, bool collision);
        public const string DetachEntity = "fivepd:cfx:DetachEntity";

        // void SetPedIntoVehicle(int ped,int vehicle,int seatIndex);
        public const string SetPedIntoVehicle = "fivepd:cfx:SetPedIntoVehicle";

        // void ResurrectPed(int ped);
        public const string ResurrectPed = "fivepd:cfx:ResurrectPed";

        // void SetEnableHandcuffs(int ped, bool toggle);
        public const string SetEnableHandcuffs = "fivepd:cfx:SetEnableHandcuffs";

        // void TaskVehiclePark(int ped, int vehicle, float x, float y, float z, float heading, int mode, float radius, bool keepEngineOn);
        public const string TaskVehiclePark = "fivepd:cfx:TaskVehiclePark";

        // void TaskVehicleTempAction(int driver, int vehicle, int action, int time);
        public const string TaskVehicleTempAction = "fivepd:cfx:TaskVehicleTempAction";

        // void SetEntityAsMissionEntity(int entity, bool p1, bool p2);
        public const string SetEntityAsMissionEntity = "fivepd:cfx:SetEntityAsMissionEntity";
    }
}
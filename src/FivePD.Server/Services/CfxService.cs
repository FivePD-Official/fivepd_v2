using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class CfxService : ICfxService
{
    private readonly IEventService _eventService;

    public CfxService(IEventService eventService)
    {
        this._eventService = eventService;
    }

    public void AddAmmoToPed(int ped, uint weaponHash, int ammo)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(ped).Owner, Events.Cfx.AddAmmoToPed, weaponHash, ammo);
    }

    public void AddAmmoToPedByType(int ped, int ammoType, int ammo)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(ped).Owner, Events.Cfx.AddAmmoToPedByType, ammoType, ammo);
    }

    public void AddArmourToPed(int ped, int amount)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(ped).Owner, Events.Cfx.AddArmourToPed, ped, amount);
    }

    public void AddPedAmmo(int ped, int ammoType, int ammo)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(ped).Owner, Events.Cfx.AddPedAmmo, ped, ammoType, ammo);
    }

    public void ApplyDamageToPed(int ped, int damageAmount, bool armorFirst)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(ped).Owner, Events.Cfx.ApplyDamageToPed, ped, damageAmount, armorFirst);
    }

    public void ApplyForceToEntity(
        int entity,
        int forceType,
        float x,
        float y,
        float z,
        float offX,
        float offY,
        float offZ,
        int boneIndex,
        bool isDirectionRel,
        bool ignoreUpVec,
        bool isForceRel,
        bool p12,
        bool p13)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(entity).Owner, Events.Cfx.ApplyForceToEntity, entity, forceType, x, y, z, offX,
            offY, offZ,
            boneIndex, isDirectionRel, ignoreUpVec, isForceRel, p12, p13);
    }

    public void ApplyForceToEntityCenterOfMass(int entity, int forceType, float x, float y, float z, bool p5, bool isDirectionRel, bool isForceRel, bool p8)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(entity).Owner, Events.Cfx.ApplyForceToEntityCenterOfMass, entity, forceType, x, y,
            z, p5, isDirectionRel, isForceRel, p8);
    }

    public async Task<FVehicle> GetClosestVehicle(Vector3 location, float radius)
    {
        var vehicleNetworkId = await this._eventService.TriggerEventAsync(null, Events.Cfx.GetClosestVehicle, location, radius);
        var vehicle = Entity.FromNetworkId(int.Parse(vehicleNetworkId)) as Vehicle;
        return FVehicle.FromVehicle(vehicle);
    }

    public void AttachEntityToEntity(
        int entity1,
        int entity2,
        int boneIndex,
        float xPos,
        float yPos,
        float zPos,
        float xRot,
        float yRot,
        float zRot,
        bool p9,
        bool useSoftPinning,
        bool collision,
        bool isPed,
        int rotationOrder,
        bool fixedRot)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(entity1).Owner, Events.Cfx.AttachEntityToEntity, entity1, entity2, boneIndex, xPos,
            yPos, zPos, xRot, yRot, zRot, p9,
            useSoftPinning, collision, isPed, rotationOrder, fixedRot);
    }

    public void DetachEntity(int entity, bool _dynamic, bool collision)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(entity).Owner, Events.Cfx.DetachEntity, entity, _dynamic, collision);
    }

    public void SetPedIntoVehicle(int ped, int vehicle, int seatIndex)
    {
        BaseScript.TriggerClientEvent(Events.Cfx.SetPedIntoVehicle, ped, vehicle, seatIndex);
    }

    public void ResurrectPed(int ped)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(ped).Owner, Events.Cfx.ResurrectPed, ped);
    }

    public void SetEnableHandcuffs(int ped, bool toggle)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(ped).Owner, Events.Cfx.SetEnableHandcuffs, ped, toggle);
    }

    public void TaskVehiclePark(int ped, int vehicle, float x, float y, float z, float heading, int mode, float radius, bool keepEngineOn)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(ped).Owner, Events.Cfx.TaskVehiclePark, ped, vehicle, x, y, z, heading, mode, radius,
            keepEngineOn);
    }

    public void TaskVehicleTempAction(int driver, int vehicle, int action, int time)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(driver).Owner, Events.Cfx.TaskVehicleTempAction, driver, vehicle, action, time);
    }

    public void SetEntityAsMissionEntity(int entity, bool p1, bool p2)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(entity).Owner, Events.Cfx.SetEntityAsMissionEntity, entity, p1, p2);
    }
    
    public void PlaceObjectOnGroundProperly(int entity)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(entity).Owner, Events.Cfx.PlaceObjectOnGroundProperly, entity);
    }

    public void TaskShootAtEntity(int entity, int target, int duration, uint firingPattern)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(entity).Owner, Events.Cfx.TaskShootAtEntity, entity, target, duration, firingPattern);
    }
    
    public void SetPedFleeAttributes(int entity, int attributeFlags, bool enable)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(entity).Owner, Events.Cfx.SetPedFleeAttributes, entity, attributeFlags, enable);
    }
    
    public void SetPedCombatAttributes(int entity, int attributeIndex, bool enabled)
    {
        this._eventService.TriggerEventWithoutCallback(Entity.FromNetworkId(entity).Owner, Events.Cfx.SetPedCombatAttributes, entity, attributeIndex, enabled);
    }
}
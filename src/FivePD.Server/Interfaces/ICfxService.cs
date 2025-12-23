using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers.VehicleWrapper;

namespace FivePD.Server.Interfaces;

#pragma warning disable

/// <summary>
/// Provides an interface for interacting with CitizenFX natives.
/// </summary>
public interface ICfxService
{
    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void AddAmmoToPed(int ped, uint weaponHash, int ammo);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void AddAmmoToPedByType(int ped, int ammoType, int ammo);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void AddArmourToPed(int ped, int amount);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void AddPedAmmo(int ped, int ammoType, int ammo);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void ApplyDamageToPed(int ped, int damageAmount, bool armorFirst);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
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
        bool p13);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void ApplyForceToEntityCenterOfMass(int entity, int forceType, float x, float y, float z, bool p5, bool isDirectionRel, bool isForceRel, bool p8);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public Task<FVehicle> GetClosestVehicle(Vector3 location, float radius);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
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
        bool fixedRot);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void DetachEntity(int entity, bool _dynamic, bool collision);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void SetPedIntoVehicle(int ped, int vehicle, int seatIndex);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void ResurrectPed(int ped);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void SetEnableHandcuffs(int ped, bool toggle);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void TaskVehiclePark(int ped, int vehicle, float x, float y, float z, float heading, int mode, float radius, bool keepEngineOn);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void TaskVehicleTempAction(int driver, int vehicle, int action, int time);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void SetEntityAsMissionEntity(int entity, bool p1, bool p2);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void PlaceObjectOnGroundProperly(int entity);

    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void TaskShootAtEntity(int entity, int target, int duration, uint firingPattern);
    
    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void SetPedFleeAttributes(int entity, int attributeFlags, bool enable);
    
    /// <summary>
    /// Please refer to the FiveM API documentation. This method invokes the same method on the client side.
    /// </summary>
    public void SetPedCombatAttributes(int entity, int attributeIndex, bool enabled);
}
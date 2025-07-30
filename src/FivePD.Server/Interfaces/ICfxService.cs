using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.VehicleWrapper;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface ICfxService
{
    public void AddAmmoToPed(int ped, uint weaponHash, int ammo);

    public void AddAmmoToPedByType(int ped, int ammoType, int ammo);

    public void AddArmourToPed(int ped, int amount);

    public void AddPedAmmo(int ped, int ammoType, int ammo);

    public void ApplyDamageToPed(int ped, int damageAmount, bool armorFirst);

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

    public void ApplyForceToEntityCenterOfMass(int entity, int forceType, float x, float y, float z, bool p5, bool isDirectionRel, bool isForceRel, bool p8);

    public Task<FVehicle> GetClosestVehicle(Vector3 location, float radius);

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

    public void DetachEntity(int entity, bool _dynamic, bool collision);

    public void SetPedIntoVehicle(int ped, int vehicle, int seatIndex);

    public void ResurrectPed(int ped);

    public void SetEnableHandcuffs(int ped, bool toggle);

    public void TaskVehiclePark(int ped, int vehicle, float x, float y, float z, float heading, int mode, float radius, bool keepEngineOn);

    public void TaskVehicleTempAction(int driver, int vehicle, int action, int time);

    public void SetEntityAsMissionEntity(int entity, bool p1, bool p2);

    public void PlaceObjectOnGroundProperly(int entity);
}
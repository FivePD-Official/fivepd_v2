using System;
using System.Linq;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

#pragma warning disable

internal class CfxService : ICfxService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IQueueService _queueService;

    public CfxService(EventHandlerDictionary eventHandlerDictionary, IQueueService queueService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._queueService = queueService;
    }

    public void OnInit()
    {
        this._eventHandlerDictionary[Events.Cfx.AddAmmoToPed] += new Action<int, uint, int>((ped, weaponHash, ammo) =>
        {
            var entity = this.GetEntity(ped);
            if (entity is null)
            {
                return;
            }
            
            this._queueService.Queue(() => Cfx.API.AddAmmoToPed(entity.Handle, weaponHash, ammo));
        });

        this._eventHandlerDictionary[Events.Cfx.AddAmmoToPedByType] += new Action<int, int, int>((ped, ammoType, ammo) =>
        {
            var entity = this.GetEntity(ped);
            if (entity is null)
            {
                return;
            }

            this._queueService.Queue(() => Cfx.API.AddAmmoToPedByType(entity.Handle, ammoType, ammo));
        });

        this._eventHandlerDictionary[Events.Cfx.AddArmourToPed] += new Action<int, int>((ped, amount) =>
        {
            var entity = this.GetEntity(ped);
            if (entity is null)
            {
                return;
            }

            this._queueService.Queue(() => Cfx.API.AddArmourToPed(entity.Handle, amount));
        });

        this._eventHandlerDictionary[Events.Cfx.AddPedAmmo] += new Action<int, int, int>((ped, ammoType, amount) =>
        {
            var entity = this.GetEntity(ped);
            if (entity is null)
            {
                return;
            }

            this._queueService.Queue(() => Cfx.API.AddPedAmmo(entity.Handle, ammoType, amount));
        });

        this._eventHandlerDictionary[Events.Cfx.ApplyDamageToPed] += new Action<int, int, bool>((ped, damageAmount, armorFirst) =>
        {
            var entity = this.GetEntity(ped);
            if (entity is null)
            {
                return;
            }

            this._queueService.Queue(() => Cfx.API.ApplyDamageToPed(entity.Handle, damageAmount, armorFirst));
        });

        this._eventHandlerDictionary[Events.Cfx.ApplyForceToEntity] +=
            new Action<int, int, float, float, float, float, float, float, int, bool, bool, bool, bool, bool>(
                (entityId, forceType, x, y, z, offX, offY, offZ, boneIndex, isDirectionRel, ignoreUpVec, isForceRel, p12, p13) =>
                {
                    var entity = this.GetEntity(entityId);
                    if (entity is null)
                    {
                        return;
                    }

                    this._queueService.Queue(() => Cfx.API.ApplyForceToEntity(entity.Handle, forceType, x, y, z, offX, offY, offZ, boneIndex, isDirectionRel,
                        ignoreUpVec, isForceRel, p12,
                        p13));
                });

        this._eventHandlerDictionary[Events.Cfx.ApplyForceToEntityCenterOfMass] += new Action<int, int, float, float, float, bool, bool, bool, bool>(
            (entityId, forceType, x, y, z, p5, isDirectionRel, isForceRel, p8) =>
            {
                var entity = this.GetEntity(entityId);
                if (entity is null)
                {
                    return;
                }

                this._queueService.Queue(() => Cfx.API.ApplyForceToEntityCenterOfMass(entity.Handle, forceType, x, y, z, p5, isDirectionRel, isForceRel, p8));
            });

        this._eventHandlerDictionary[Events.Cfx.GetClosestVehicle] += new Action<string, Vector3, float>((callbackId, location, radius) =>
        {
            Debug.WriteLine("asdasd");
            Debug.WriteLine($"{callbackId}");
            Debug.WriteLine($"{location}");
            Debug.WriteLine($"{radius}");
            var closestVehicle = World.GetAllVehicles().FirstOrDefault(x => World.GetDistance(x.Position, location) <= radius);
            BaseScript.TriggerServerEvent(Events.Generic.Callback, callbackId, closestVehicle?.NetworkId ?? 0);
        });

        this._eventHandlerDictionary[Events.Cfx.AttachEntityToEntity] += new Action<
            int,
            int,
            int,
            float,
            float,
            float,
            float,
            float,
            float,
            bool,
            bool,
            bool,
            bool,
            int,
            bool>((i, i1, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) =>
        {
            var entity1 = this.GetEntity(i);
            var entity2 = this.GetEntity(i1);
            if (entity1 is null || entity2 is null)
            {
                return;
            }

            this._queueService.Queue(() =>
                Cfx.API.AttachEntityToEntity(entity1.Handle, entity2.Handle, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14,
                    arg15));
        });

        this._eventHandlerDictionary[Events.Cfx.DetachEntity] += new Action<
            int,
            bool,
            bool>((arg1, arg2, arg3) =>
        {
            var entity1 = this.GetEntity(arg1);
            if (entity1 is null)
            {
                return;
            }

            this._queueService.Queue(() => Cfx.API.DetachEntity(entity1.Handle, arg2, arg3));
        });

        this._eventHandlerDictionary[Events.Cfx.SetPedIntoVehicle] += new Action<
            int,
            int,
            int>((i, i1, i2) =>
        {
            var entity1 = this.GetEntity(i);
            if (entity1 is null)
            {
                return;
            }

            this._queueService.Queue(() => Cfx.API.SetPedIntoVehicle(entity1.Handle, i1, i2));
        });

        this._eventHandlerDictionary[Events.Cfx.ResurrectPed] += new Action<int>((ped) =>
        {
            var entity = this.GetEntity(ped);
            if (entity is null)
            {
                return;
            }

            this._queueService.Queue(() => Cfx.API.ResurrectPed(entity.Handle));
        });

        this._eventHandlerDictionary[Events.Cfx.SetEnableHandcuffs] += new Action<int, bool>((ped, toggle) =>
        {
            var entity = this.GetEntity(ped);
            if (entity is null)
            {
                return;
            }

            this._queueService.Queue(() => Cfx.API.SetEnableHandcuffs(entity.Handle, toggle));
        });

        this._eventHandlerDictionary[Events.Cfx.TaskVehiclePark] += new Action<int, int, float, float, float, float, int, float, bool>(
            (pedNetId, vehicleNetId, x, y, z, heading, mode, radius, keepEngineOn) =>
            {
                var ped = this.GetEntity(pedNetId);
                var vehicle = this.GetEntity(vehicleNetId);
                if (ped is null || vehicle is null)
                {
                    return;
                }

                this._queueService.Queue(() => Cfx.API.TaskVehiclePark(ped.Handle, vehicle.Handle, x, y, z, heading, mode, radius, keepEngineOn));
            });
        
        this._eventHandlerDictionary[Events.Cfx.TaskVehicleTempAction] += new Action<int, int, int, int>(
            (pedNetId, vehicleNetId, action, time) =>
            {
                var ped = this.GetEntity(pedNetId);
                var vehicle = this.GetEntity(vehicleNetId);
                if (ped is null || vehicle is null)
                {
                    return;
                }

                this._queueService.Queue(() => Cfx.API.TaskVehicleTempAction(ped.Handle, vehicle.Handle, action, time));
            });
        
        this._eventHandlerDictionary[Events.Cfx.SetEntityAsMissionEntity] += new Action<int, bool, bool>(
            (entityNetId, p1, p2) =>
            {
                var entity = this.GetEntity(entityNetId);
                if (entity is null)
                {
                    return;
                }

                this._queueService.Queue(() => CitizenFX.Core.Native.API.SetEntityAsMissionEntity(entity.Handle, p1, p2));
            });
    }

    private Entity GetEntity(int networkId)
    {
        var entity = Entity.FromNetworkId(networkId);
        if (entity is null)
        {
            return null;
        }
        
        Cfx.API.SetEntityLoadCollisionFlag(entity.Handle, true);

        return entity;
    }
}
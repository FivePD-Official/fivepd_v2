using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.EntityWrappers.VehicleWrapper;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.Services;

#pragma warning disable

public class CfxService : ICfxService
{
    private readonly IEventService _eventService;

    public CfxService(IEventService eventService)
    {
        this._eventService = eventService;
    }

    public async Task<FVehicle> GetClosestVehicle(FPlayer player, Vector3 location, float radius)
    {
        Debug.WriteLine("b");
        var vehicleNetworkId = await this._eventService.TriggerEventStrictAsync(player.GetUnderlyingPlayer(), "fivepd:cfx:getClosestVehicle", location, radius);
        var vehicle = Entity.FromNetworkId(int.Parse(vehicleNetworkId)) as Vehicle;
        Debug.WriteLine("a");
        return FVehicle.FromVehicle(vehicle);
    }

    public void AttachEntityToEntity(
        FPlayer player,
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
        int vertexIndex,
        bool fixedRot)
    {
        player.GetUnderlyingPlayer().TriggerEvent(
            "fivepd:cfx:AttachEntityToEntity",
            entity1,
            entity2,
            boneIndex,
            xPos,
            yPos,
            zPos,
            xRot,
            yRot,
            zRot,
            p9,
            useSoftPinning,
            collision,
            isPed,
            vertexIndex,
            fixedRot);
    }
}
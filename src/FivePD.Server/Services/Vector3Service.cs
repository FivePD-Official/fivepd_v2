using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.DTO;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class Vector3Service : IVector3Service
{
    private readonly IEventService _eventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3Service"/> class.
    /// </summary>
    /// <param name="eventService">The <see cref="IEventService"/> to use.</param>
    public Vector3Service(IEventService eventService)
    {
        this._eventService = eventService;
    }

    /// <inheritdoc />
    public async Task<Vector3> GetRandomLocationAsync(Entity entity, int offsetMin, int offsetMax)
    {
        if (entity is null)
        {
            return default;
        }

        var serializedResponse = await this._eventService.TriggerEventAsync(
            null,
            Events.Vector3.GetRandomOffset,
            JsonConvert.SerializeObject(new RandomOffsetDTO
            {
                EntityNetworkId = entity.NetworkId,
                OffsetMin = offsetMin,
                OffsetMax = offsetMax,
            }));
        return JsonConvert.DeserializeObject<Vector3>(serializedResponse);
    }

    /// <inheritdoc />
    public async Task<Vector3WithHeading> GetPointOnRoadSideAsync(Entity entity, int offsetMin, int offsetMax)
    {
        if (entity is null)
        {
            return default;
        }

        var serializedResponse = await this._eventService.TriggerEventAsync(
            null,
            Events.Vector3.GetPointOnRoadSide,
            JsonConvert.SerializeObject(new RandomOffsetDTO
            {
                EntityNetworkId = entity.NetworkId,
                OffsetMin = offsetMin,
                OffsetMax = offsetMax,
            }));
        return JsonConvert.DeserializeObject<Vector3WithHeading>(serializedResponse);
    }
}
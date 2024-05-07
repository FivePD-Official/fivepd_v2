using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
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
    public async Task<Vector3> GetRandomLocationAsync(Entity entity)
    {
        if (entity is null)
        {
            return default;
        }

        var serializedVector3 = await this._eventService.TriggerEventAsync(null, Events.Vector3.GetRandomOffset, entity.NetworkId);
        var location = JsonConvert.DeserializeObject<Vector3>(serializedVector3);
        return location;
    }

    /// <inheritdoc />
    public async Task<Vector3> GetPointOnRoadSideAsync(Entity entity, int p3)
    {
        var serializedVector3 = await this._eventService.TriggerEventAsync(null, Events.Vector3.GetPointOnRoadSide, entity.NetworkId, p3);
        var point = JsonConvert.DeserializeObject<Vector3>(serializedVector3);
        return point;
    }
}
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Interfaces;

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
    public async Task<string> GetRandomLocationAsync(Entity entity)
    {
        if (entity is null)
        {
            return default;
        }

        var serializedJson = await this._eventService.TriggerEventAsync(null, Events.Vector3.GetRandomOffset, entity.NetworkId);
        return serializedJson;
    }

    /// <inheritdoc />
    public async Task<string> GetPointOnRoadSideAsync(Entity entity)
    {
        var serializedJson = await this._eventService.TriggerEventAsync(null, Events.Vector3.GetPointOnRoadSide, entity.NetworkId);
        return serializedJson;
    }
}
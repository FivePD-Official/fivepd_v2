using System.Dynamic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.Requests;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class DevToolsService : IDevToolsService
{
    private readonly INetworkEventService _networkCallbackDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevToolsService"/> class.
    /// </summary>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public DevToolsService(INetworkEventService networkEventService)
    {
        this._networkCallbackDelegate = networkEventService;
    }

    [UIEvent(Events.DevTools.DeleteEntities)]
    private void DeleteEntitiesFromUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<DeleteEntitiesRequest>(serializedRequest);
        switch (request.Type)
        {
            case DeleteEntitiesRequest.DeleteEntityType.All:
            {
                this.DeleteAllEntities();
                break;
            }

            case DeleteEntitiesRequest.DeleteEntityType.Peds:
            {
                this.DeletePeds();
                break;
            }

            case DeleteEntitiesRequest.DeleteEntityType.Vehicles:
            {
                this.DeleteVehicles();
                break;
            }

            case DeleteEntitiesRequest.DeleteEntityType.Props:
            {
                this.DeleteProps();
                break;
            }
        }

        this._networkCallbackDelegate.TriggerEvent(player, guid, null);
    }

    private void DeleteAllEntities()
    {
        this.DeletePeds();
        this.DeleteVehicles();
        this.DeleteProps();
    }

    private void DeletePeds()
    {
        foreach (int entity in Cfx.API.GetAllPeds())
        {
            Cfx.API.DeleteEntity(entity);
        }
    }

    private void DeleteVehicles()
    {
        foreach (int entity in Cfx.API.GetAllVehicles())
        {
            Cfx.API.DeleteEntity(entity);
        }
    }

    private void DeleteProps()
    {
        foreach (int entity in Cfx.API.GetAllObjects())
        {
            Cfx.API.DeleteEntity(entity);
        }
    }
}
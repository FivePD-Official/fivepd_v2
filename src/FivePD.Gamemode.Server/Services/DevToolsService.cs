using System.Dynamic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models.Requests;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services
{
    /**
     * <inheritdoc cref="IDevToolsService" />
     */
    public class DevToolsService : IDevToolsService
    {
        [UIEvent(Events.DevTools.DeleteEntities)]
        private void DeleteEntitiesFromUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var request = expandoObject.ConvertTo<DeleteEntitiesRequest>();
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

            networkCallbackDelegate.Invoke(null);
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
}
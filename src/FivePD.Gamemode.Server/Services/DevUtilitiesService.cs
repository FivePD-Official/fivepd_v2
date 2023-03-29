using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Attributes;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models.Requests;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Server.Services
{
    /**
     * <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IDevUtilitiesService" />
     */
    public class DevUtilitiesService : IDevUtilitiesService
    {
        private readonly ICommandService _commandService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevUtilitiesService"/> class.
        /// </summary>
        /// <param name="commandService">The <see cref="ICommandService"/> to use.</param>
        public DevUtilitiesService(ICommandService commandService)
        {
            this._commandService = commandService;
        }

        /**
         * <inheritdoc />
         */
        [NoUsage]
        public void RegisterDevelopmentCommands()
        {
            this._commandService.RegisterCommand("clear", (source, args) => this.DeleteAllEntities());
        }

        [UIEvent(Events.DevTools.DeleteEntities)]
        private void DeleteEntitiesFromUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
        {
            var request = expandoObject.ConvertTo<DeleteEntitiesRequest>();
            switch (request.Type)
            {
                case "all":
                {
                    this.DeleteAllEntities();
                    break;
                }

                case "peds":
                {
                    this.DeletePeds();
                    break;
                }

                case "vehicles":
                {
                    this.DeleteVehicles();
                    break;
                }

                case "props":
                {
                    this.DeleteProps();
                    break;
                }
            }

            // TODO: response title and description
            networkCallbackDelegate.InvokeWithResponse(true, string.Empty, string.Empty);
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
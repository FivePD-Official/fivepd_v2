using System.Dynamic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

public class VehicleConfigService : IVehicleConfigService
{
    private readonly DatabaseContext _databaseContext;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    
    public VehicleConfigService(DatabaseContext databaseContext, EventHandlerDictionary eventHandlerDictionary)
    {
        this._databaseContext = databaseContext;
        this._eventHandlerDictionary = eventHandlerDictionary;
    }
   
    [UIEvent(Events.UI.VehicleConfig.Add)]
    private void GetDepartmentRankForNui(ExpandoObject requestString, NetworkCallbackDelegate networkCallbackDelegate)
    {
         var request = requestString.ConvertTo<AddVehicleConfigRequest>();
         this._databaseContext.VehicleConfigs.Add(new VehicleConfig {
             Type = request.Type,
             Name = request.Name,
             Model = request.Model,
             Livery = request.Livery,
             Extras = request.Extras,
             Options = request.Options,
         });
        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "a", "b");
    }   
}
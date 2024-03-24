using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

public class VehicleConfigService : IVehicleConfigService
{
    private readonly ILogger _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly IMenuService _menuService;

    public VehicleConfigService(ILogger logger, DatabaseContext databaseContext, IMenuService menuService)
    {
        this._logger = logger;
        this._databaseContext = databaseContext;
        this._menuService = menuService;
    }

    public List<VehicleConfig<Dictionary<string, object>>> GetAll(VehicleConfigType type)
    {
        return this._databaseContext.VehicleConfigs.Where(x => x.Type == type).ToList();
    }
    
    public List<VehicleConfig<TOptions>> GetAll<TOptions>(VehicleConfigType type)
    {
        var vehicleConfigs = this._databaseContext.VehicleConfigs.Where(x => x.Type == type).ToList();

        try
        {
            return vehicleConfigs.Select(vehicleConfig => new VehicleConfig<TOptions>
                {
                    Id = vehicleConfig.Id,
                    Name = vehicleConfig.Name,
                    Type = vehicleConfig.Type,
                    Livery = vehicleConfig.Livery,
                    Model = vehicleConfig.Model,
                    Extras = vehicleConfig.Extras,
                    Options = JsonConvert.DeserializeObject<TOptions>(JsonConvert.SerializeObject(vehicleConfig.Options))
                })
                .ToList();
        }
        catch (Exception e)
        {
            this._logger.Error("Could not map a vehicle config's Options field to the given type. Exception: {Exception}", e);
        }

        return new List<VehicleConfig<TOptions>>();
    }

    [UIEvent(Events.UI.VehicleConfig.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var vehicleConfig = this._databaseContext.VehicleConfigs.FirstOrDefault(x => x.Id == request.Id);
        if (vehicleConfig is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }
        
        networkCallbackDelegate.InvokeWithJson(vehicleConfig);
    }
    
    [UIEvent(Events.UI.VehicleConfig.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<VehicleConfigsRequest>();
        var filters = new Filters<VehicleConfig<Dictionary<string, object>>>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            vehicleConfig => vehicleConfig.Name.ToLower().Contains(request.Filters.Name.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Model),
            vehicleConfig => vehicleConfig.Model.ToLower().Contains(request.Filters.Model.ToLower()));
        filters.Add(request.Filters.Type != -1, vehicleConfig => vehicleConfig.Type == (VehicleConfigType)request.Filters.Type);
        var page = this._databaseContext.VehicleConfigs
            .AsNoTracking()
            .Paginate(request.Page, filters);

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.VehicleConfig.Create)]
    private void CreateVehicleConfigUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<AddVehicleConfigRequest>();
        this._databaseContext.VehicleConfigs.Add(new VehicleConfig<Dictionary<string, object>>
        {
            Type = request.Type,
            Name = request.Name,
            Model = request.Model,
            Livery = request.Livery,
            Extras = request.Extras,
            Options = request.Options,
        });
        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.CREATE", "UI.LABEL.VEHICLE_CONFIG.SUCCESS.CREATE");
    }
    
    [UIEvent(Events.UI.VehicleConfig.Edit)]
    private void EditVehicleConfigUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<EditVehicleConfigRequest>();
        var vehicleConfig = this._databaseContext.VehicleConfigs.FirstOrDefault(x => x.Id == request.Id);
        if (vehicleConfig is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        vehicleConfig.Name = request.Name;
        vehicleConfig.Model = request.Model;
        vehicleConfig.Livery = request.Livery;
        vehicleConfig.Extras = request.Extras;
        vehicleConfig.Options = request.Options;

        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.EDIT", "UI.LABEL.VEHICLE_CONFIG.SUCCESS.EDIT");
    }

    [UIEvent(Events.UI.VehicleConfig.Delete)]
    private void DeleteVehicleConfigUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var entityToRemove = this._databaseContext.VehicleConfigs.FirstOrDefault(x => x.Id == request.Id);
        if (entityToRemove is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        this._databaseContext.VehicleConfigs.Remove(entityToRemove);
        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.VEHICLE_CONFIG.SUCCESS.DELETE");
    }
}
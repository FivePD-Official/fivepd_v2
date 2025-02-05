using System;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Enums;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IPedService" />
[Obfuscate]
public class PedService : IPedService, IOnInit, IOnDestroy
{
    private readonly IInternalPedStorageService _pedStorageService;
    private readonly IVehicleStorageService _vehicleStorageService;
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IConfigService _configService;
    private readonly DatabaseContext _databaseContext;
    private readonly IRandomService _randomService;
    private readonly ITickQueueService _tickQueueService;

    private Guid _checkPedsId;

    /// <summary>
    /// Initializes a new instance of the <see cref="PedService"/> class.
    /// </summary>
    /// <param name="pedStorageService">The <see cref="IPedStorageService" /> to use.</param>
    /// <param name="vehicleStorageService">The <see cref="IVehicleStorageService" /> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService" /> to use.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext" /> to use.</param>
    /// <param name="randomService">The <see cref="IRandomService" /> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService" /> to use.</param>
    public PedService(
        IPedStorageService pedStorageService,
        IVehicleStorageService vehicleStorageService,
        EventHandlerDictionary eventHandlerDictionary,
        IConfigService configService,
        DatabaseContext databaseContext,
        IRandomService randomService,
        ITickQueueService tickQueueService)
    {
        this._pedStorageService = (IInternalPedStorageService)pedStorageService;
        this._vehicleStorageService = vehicleStorageService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
        this._databaseContext = databaseContext;
        this._randomService = randomService;
        this._tickQueueService = tickQueueService;
    }

    /// <inheritdoc/>
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Ped.Generate] += new Action<string>(this.GeneratePed);
        this._checkPedsId = this._tickQueueService.EnqueueInterval(this.CheckPeds, 1000);
    }

    /// <inheritdoc/>
    void IOnDestroy.OnDestroy()
    {
        this._tickQueueService.DequeueInterval(this._checkPedsId);
    }

    private void CheckPeds()
    {
        foreach (var keyValuePair in this._pedStorageService.InternalPeds)
        {
            var underlyingPed = keyValuePair.Value.GetUnderlyingPed;
            if (underlyingPed is null || !Cfx.API.DoesEntityExist(underlyingPed.Handle))
            {
                this._pedStorageService.InternalPeds.TryRemove(keyValuePair.Key, out _);
            }
        }
    }

#pragma warning disable

    private void GeneratePed(string serializedFPedBuilder)
    {
        var commonFPedBuilder = JsonConvert.DeserializeObject<CommonFPedBuilder>(serializedFPedBuilder);
        if (commonFPedBuilder is null || this._pedStorageService.InternalPeds.ContainsKey(commonFPedBuilder.NetworkId))
        {
            return;
        }

        var fPedBuilder = new FPedBuilder(commonFPedBuilder);

        var owningFVehicle = this._vehicleStorageService.TryGetVehicle(commonFPedBuilder.OwningVehicleNetworkId);
        var vehicleData = owningFVehicle is null
            ? null
            : this._databaseContext.VehicleData.FirstOrDefault(x => x.Id == owningFVehicle.VehicleDataId);

        FPed fPed = null;
        if (this._configService.GeneralConfig.Peds.ChanceToReuseData != 0 &&
            this._randomService.GetRandomChance <= this._configService.GeneralConfig.Peds.ChanceToReuseData)
        {
            var pedData = this._databaseContext.PedData
                .OrderBy(_ => Guid.NewGuid())
                .Include(pedHistory => pedHistory.Licenses)
                .FirstOrDefault(ped => ped.Gender == commonFPedBuilder.Gender);
            if (pedData is not null)
            {
                fPed = fPedBuilder
                    .WithOwningVehicleNetworkId(commonFPedBuilder.OwningVehicleNetworkId)
                    .WithFirstname(pedData.Firstname)
                    .WithLastname(pedData.Lastname)
                    .WithBirthdate(pedData.Birthdate)
                    .WithItems(pedData.Items, true)
                    .WithWarrants(new FPedBuilder.WithWarrantsOptions
                    {
                        Warrants = pedData.Warrants,
                        IncludeAll = true,
                    })
                    .WithLicenses(pedData.Licenses)
                    .WithAlcoholLevel(pedData.AlcoholLevel)
                    .WithDrugs(pedData.Drugs)
                    .WithPedData(pedData)
                    .Build();

                if (vehicleData is not null)
                {
                    vehicleData.PedDataId = pedData.Id;
                    this._databaseContext.SaveChanges();
                }
            }
        }

        if (fPed is null)
        {
            fPedBuilder
                .WithOwningVehicleNetworkId(commonFPedBuilder.OwningVehicleNetworkId)
                .WithFirstname(commonFPedBuilder.Gender == FGender.Female ? this._configService.FemaleFirstnames : this._configService.MaleFirstnames)
                .WithLastname(this._configService.Lastnames)
                .WithBirthdate()
                .WithItems(this._configService.Items)
                .WithWarrants(new FPedBuilder.WithWarrantsOptions
                {
                    Warrants = this._configService.GeneralConfig.Peds.Warrants.Items,
                    IncludeAll = false,
                    MaxWarrants = this._configService.GeneralConfig.Peds.Warrants.MaxAmount,
                    ChanceForAnyWarrant = this._configService.GeneralConfig.Peds.Warrants.ChanceToHaveAny,
                })
                .WithRandomLicenses(this._configService.GeneralConfig.Peds.Licenses)
                .WithAlcoholLevel(this._configService.GeneralConfig.Peds.Alcohol)
                .WithDrugs(this._configService.GeneralConfig.Peds.Drugs);
            var entry = this._databaseContext.PedData.Add(new PedData
            {
                Firstname = fPedBuilder.Firstname,
                Lastname = fPedBuilder.Lastname,
                Birthdate = fPedBuilder.Birthdate,
                Gender = fPedBuilder.Gender,
                Items = fPedBuilder.Items,
                Warrants = fPedBuilder.Warrants,
                Licenses = fPedBuilder.Licenses,
                AlcoholLevel = fPedBuilder.AlcoholLevel,
                Drugs = fPedBuilder.Drugs,
                VehicleDataId = vehicleData?.Id,
            });
            this._databaseContext.SaveChanges();

            if (vehicleData is not null)
            {
                vehicleData.PedDataId = entry.Entity.Id;
                this._databaseContext.SaveChanges();
            }

            fPed = fPedBuilder
                .WithPedData(entry.Entity)
                .Build();

            fPed.PedDataId = entry.Entity.Id;
        }

        Debug.WriteLine($"Generated data for ped with net ID of {fPed.NetworkId} - Name: {fPedBuilder.Firstname} {fPedBuilder.Lastname}");

        fPed.HasGeneratedData.Value = true;
        this._pedStorageService.InternalPeds[fPed.NetworkId] = fPed;
    }

    [UIEvent(Events.PedHistory.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<PedHistoryGetAllRequest>();

        var filters = new Filters<PedData>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Firstname),
            pedHistory => pedHistory.Firstname.ToLower().Contains(request.Filters.Firstname.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Lastname),
            pedHistory => pedHistory.Lastname.ToLower().Contains(request.Filters.Lastname.ToLower()));
        filters.Add(
            request.Filters.Gender != -1,
            pedHistory => pedHistory.Gender == (FGender)request.Filters.Gender);

        var page = this._databaseContext.PedData
            .AsNoTracking()
            .Paginate(request.Page, filters);

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.PedHistory.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var pedData = this._databaseContext.PedData
            .AsNoTracking()
            .Include(x => x.VehicleData)
            .Include(x => x.Licenses)
            .FirstOrDefault(x => x.Id == request.Id);
        networkCallbackDelegate.InvokeWithJson(pedData);
    }

    [UIEvent(Events.PedHistory.GetAllSimple)]
    private void GetAllSimpleUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var simplePedHistories = this._databaseContext.PedData
            .AsNoTracking()
            .Select(x => new SimplePedHistoryDTO
            {
                Id = x.Id,
                Birthdate = x.Birthdate,
                Gender = x.Gender,
                Lastname = x.Lastname,
                Firstname = x.Firstname,
            })
            .ToList();

        networkCallbackDelegate.InvokeWithJson(simplePedHistories);
    }
}
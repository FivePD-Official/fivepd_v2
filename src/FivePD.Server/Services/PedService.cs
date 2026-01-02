using System;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Enums;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
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
    private readonly IRandomService _randomService;
    private readonly ITickQueueService _tickQueueService;
    private readonly INetworkEventService _networkEventService;
    private readonly IPedBehaviorService _pedBehaviorService;
    private readonly PedDataRepository _pedDataRepository;
    private readonly VehicleDataRepository _vehicleDataRepository;

    private Guid _checkPedsId;

    /// <summary>
    /// Initializes a new instance of the <see cref="PedService"/> class.
    /// </summary>
    /// <param name="pedStorageService">The <see cref="IPedStorageService" /> to use.</param>
    /// <param name="vehicleStorageService">The <see cref="IVehicleStorageService" /> to use.</param>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService" /> to use.</param>
    /// <param name="randomService">The <see cref="IRandomService" /> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService" /> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService" /> to use.</param>
    /// <param name="pedBehaviorService">The <see cref="IPedBehaviorService" /> to use.</param>
    /// <param name="pedDataRepository">The <see cref="PedDataRepository" /> to use.</param>
    /// <param name="vehicleDataRepository">The <see cref="VehicleDataRepository" /> to use.</param>
    public PedService(
        IPedStorageService pedStorageService,
        IVehicleStorageService vehicleStorageService,
        EventHandlerDictionary eventHandlerDictionary,
        IConfigService configService,
        IRandomService randomService,
        ITickQueueService tickQueueService,
        INetworkEventService networkEventService,
        IPedBehaviorService pedBehaviorService,
        PedDataRepository pedDataRepository,
        VehicleDataRepository vehicleDataRepository)
    {
        this._pedStorageService = (IInternalPedStorageService)pedStorageService;
        this._vehicleStorageService = vehicleStorageService;
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
        this._randomService = randomService;
        this._tickQueueService = tickQueueService;
        this._networkEventService = networkEventService;
        this._pedBehaviorService = pedBehaviorService;
        this._pedDataRepository = pedDataRepository;
        this._vehicleDataRepository = vehicleDataRepository;
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
            if (underlyingPed is null || Cfx.API.GetEntityHealth(underlyingPed.Handle) <= 0 || !Cfx.API.DoesEntityExist(underlyingPed.Handle))
            {
                this._pedBehaviorService.StopBehavior(keyValuePair.Value.NetworkId);
                keyValuePair.Value.Properties.RemoveBlips();
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
            : this._vehicleDataRepository.FindById(owningFVehicle.VehicleDataId);

        FPed fPed = null;
        if (this._configService.GeneralConfig.Peds.ChanceToReuseData != 0 &&
            this._randomService.GetRandomChance <= this._configService.GeneralConfig.Peds.ChanceToReuseData)
        {
            var pedData = this._pedDataRepository.GetRandomWithMatchingGender(commonFPedBuilder.Gender);
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
                    this._vehicleDataRepository.Update(vehicleData, x => { x.PedDataId = pedData.Id; });
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
            var pedData = this._pedDataRepository.Add(new PedData
            {
                Firstname = fPedBuilder.Firstname,
                Lastname = fPedBuilder.Lastname,
                Birthdate = fPedBuilder.Birthdate,
                Gender = fPedBuilder.Gender,
                Items = fPedBuilder.Items,
                Warrants = fPedBuilder.Warrants,
                AlcoholLevel = fPedBuilder.AlcoholLevel,
                Drugs = fPedBuilder.Drugs,
            });

            foreach (var license in fPedBuilder.Licenses)
            {
                this._pedDataRepository.AddLicense(pedData, license);
            }

            if (vehicleData is not null)
            {
                this._vehicleDataRepository.Update(vehicleData, x => { x.PedDataId = pedData.Id; });
            }

            fPed = fPedBuilder
                .WithPedData(pedData)
                .Build();

            fPed.PedDataId = pedData.Id;
        }

        Debug.WriteLine($"Generated data for ped with net ID of {fPed.NetworkId} - Name: {fPedBuilder.Firstname} {fPedBuilder.Lastname}");

        fPed.HasGeneratedData.Value = true;
        this._pedStorageService.InternalPeds[fPed.NetworkId] = fPed;
    }

    [UIEvent(Events.PedHistory.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<PedHistoryGetAllRequest>(serializedRequest);
        var page = this._pedDataRepository.Paginate(
            request.Page,
            b => b
                .If(!string.IsNullOrEmpty(request.Filters.Firstname), x => x.WhereContains("Firstname", request.Filters.Firstname))
                .If(!string.IsNullOrEmpty(request.Filters.Lastname), x => x.WhereContains("Lastname", request.Filters.Lastname))
                .If(request.Filters.Gender != -1, x => x.WhereEquals("Gender", request.Filters.Gender)));
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.PedHistory.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var pedData = this._pedDataRepository.FindByIdWithDynamicRelated(
            request.Id,
            new PedDataRepository.PedDataLoadOptions { IncludeLicenses = true, IncludeVehicleData = true });
        this._networkEventService.TriggerEventSerialized(player, guid, pedData);
    }

    [UIEvent(Events.PedHistory.GetAllSimple)]
    private void GetAllSimpleUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var simplePedHistories = this._pedDataRepository
            .GetAll()
            .Select(x => new SimplePedHistoryDTO
            {
                Id = x.Id,
                Birthdate = x.Birthdate,
                Gender = x.Gender,
                Lastname = x.Lastname,
                Firstname = x.Firstname,
            })
            .ToList();
        this._networkEventService.TriggerEventSerialized(player, guid, simplePedHistories);
    }
}
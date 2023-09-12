using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IPedService" />
public class PedService : IPedService, IServiceWithEvents, IDisposableService
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IConfigService _configService;
    private readonly DatabaseContext _databaseContext;
    private readonly IRandomService _randomService;
    private readonly ITickQueueService _tickQueueService;

    private readonly ConcurrentDictionary<int, FPed> _peds = new();
    private Guid _checkPedsId;

    /// <summary>
    /// Initializes a new instance of the <see cref="PedService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService" /> to use.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext" /> to use.</param>
    /// <param name="randomService">The <see cref="IRandomService" /> to use.</param>
    /// <param name="tickQueueService">The <see cref="ITickQueueService" /> to use.</param>
    public PedService(
        EventHandlerDictionary eventHandlerDictionary,
        IConfigService configService,
        DatabaseContext databaseContext,
        IRandomService randomService,
        ITickQueueService tickQueueService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
        this._databaseContext = databaseContext;
        this._randomService = randomService;
        this._tickQueueService = tickQueueService;
    }

    /// <inheritdoc/>
    void IServiceWithEvents.RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Ped.Generate] += new Action<string>(this.GeneratePed);
        this._checkPedsId = this._tickQueueService.EnqueueInterval(this.CheckPeds, 1000);
    }

    /// <inheritdoc/>
    void IDisposableService.InternalDispose()
    {
        this._tickQueueService.DequeueInterval(this._checkPedsId);
    }

    /// <inheritdoc/>
    public FPed TryGetPed(int networkId)
    {
        var found = this._peds.TryGetValue(networkId, out var ped);
        if (found)
        {
            return ped;
        }

        // TODO: is this needed?
        ped = FPed.FromNetId(networkId);
        this._peds[networkId] = ped;

        return ped;
    }

    private void CheckPeds()
    {
        foreach (var keyValuePair in this._peds)
        {
            var underlyingPed = keyValuePair.Value.GetUnderlyingPed;
            if (underlyingPed is null || !Cfx.API.DoesEntityExist(underlyingPed.Handle))
            {
                this._peds.TryRemove(keyValuePair.Key, out _);
            }
        }
    }

    private void GeneratePed(string serializedFPedBuilder)
    {
        var commonFPedBuilder = JsonConvert.DeserializeObject<CommonFPedBuilder>(serializedFPedBuilder);
        if (commonFPedBuilder is null || this._peds.ContainsKey(commonFPedBuilder.NetworkId))
        {
            return;
        }

        var fPedBuilder = new FPedBuilder(commonFPedBuilder);

        FPed fPed = null;
        if (this._configService.GeneralConfig.Peds.ChanceToReusePedHistory != 0 &&
            this._randomService.Random.Next(1, 101) <= this._configService.GeneralConfig.Peds.ChanceToReusePedHistory)
        {
            var pedHistory = this._databaseContext.PedHistories
                .OrderBy(_ => Guid.NewGuid())
                .Include(pedHistory => pedHistory.Licenses)
                .FirstOrDefault(ped => ped.Gender == commonFPedBuilder.Gender);
            if (pedHistory is not null)
            {
                fPed = fPedBuilder
                    .WithFirstname(pedHistory.Firstname)
                    .WithLastname(pedHistory.Lastname)
                    .WithBirthdate(pedHistory.Birthdate)
                    .WithItems(pedHistory.Items, true)
                    .WithWarrants(new FPedBuilder.WithWarrantsOptions
                    {
                        Warrants = pedHistory.Warrants,
                        IncludeAll = true,
                    })
                    .WithLicenses(pedHistory.Licenses)
                    .Build();
            }
        }

        if (fPed is null)
        {
            fPed = fPedBuilder
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
                .Build();
            this._databaseContext.PedHistories.Add(new PedHistory
            {
                Firstname = fPed.Firstname,
                Lastname = fPed.Lastname,
                Birthdate = fPed.Birthdate,
                Gender = fPed.Gender,
                Items = fPed.Items,
                Warrants = fPed.Warrants,
                Licenses = fPed.Licenses,
            });
            this._databaseContext.SaveChangesAsync();
        }

        Debug.WriteLine($"ped net id {fPed.NetworkId} - {fPed.Firstname} {fPed.Lastname}");

        fPed.HasGeneratedData.Value = true;
        this._peds[fPed.NetworkId] = fPed;
    }

    [UIEvent(Events.PedHistory.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<PedHistoryGetAllRequest>();

        var filters = new Filters<PedHistory>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Firstname),
            pedHistory => pedHistory.Firstname.ToLower().Contains(request.Filters.Firstname.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Lastname),
            pedHistory => pedHistory.Lastname.ToLower().Contains(request.Filters.Lastname.ToLower()));
        filters.Add(
            request.Filters.Gender != -1,
            pedHistory => pedHistory.Gender == (FGender)request.Filters.Gender);

        var page = this._databaseContext.PedHistories
            .AsNoTracking()
            .Paginate(request.Page, filters);

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.PedHistory.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var pedHistory = this._databaseContext.PedHistories
            .AsNoTracking()
            .Include(pedHistory => pedHistory.Licenses)
            .FirstOrDefault(x => x.Id == request.Id);
        networkCallbackDelegate.InvokeWithJson(pedHistory);
    }

    [UIEvent(Events.PedHistory.GetAllSimple)]
    private void GetAllSimpleUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var simplePedHistories = this._databaseContext.PedHistories
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
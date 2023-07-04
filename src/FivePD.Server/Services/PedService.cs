using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Builders;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Common.Models;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IPedService" />
public class PedService : IPedService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly IConfigService _configService;
    private readonly ConcurrentDictionary<int, CommonFPed> _peds = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PedService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary" /> to use.</param>
    /// <param name="configService">The <see cref="IConfigService" /> to use.</param>
    public PedService(EventHandlerDictionary eventHandlerDictionary, IConfigService configService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._configService = configService;
    }

    /// <inheritdoc/>
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Ped.Generate] += new Action<string>(this.GeneratePed);
        this._eventHandlerDictionary[Events.Ped.Search] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.Search);
    }

    /// <inheritdoc/>
    public FPed TryGetPed(int networkId)
    {
        var commonFPed = this._peds.TryGetValue(networkId, out var fPed) ? fPed : null;
        return new FPed(commonFPed);
    }

    private void GeneratePed(string serializedFPedBuilder)
    {
        var fPedBuilder = JsonConvert.DeserializeObject<CommonFPedBuilder>(serializedFPedBuilder);
        if (fPedBuilder is null || this._peds.ContainsKey(fPedBuilder.NetworkId))
        {
            return;
        }

        var fPed = fPedBuilder
            .WithFirstname(fPedBuilder.Gender == FGender.Female ? this._configService.FemaleFirstnames : this._configService.MaleFirstnames)
            .WithLastname(this._configService.Lastnames)
            .WithBirthdate()
            .WithItems(this._configService.Items)
            .Build();

        this._peds[fPed.NetworkId] = fPed;
    }

    private void Search(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<PedSearchRequest>();

        var pedsQueryable = this._peds.AsQueryable();

        if (!string.IsNullOrEmpty(request.Firstname))
        {
            pedsQueryable = pedsQueryable.Where(ped => ped.Value.Firstname.ToLower().Contains(request.Firstname.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.Lastname))
        {
            pedsQueryable = pedsQueryable.Where(ped => ped.Value.Lastname.ToLower().Contains(request.Lastname.ToLower()));
        }

        // TODO: Maybe use proper paging? Return all peds if the search params are empty?
        var peds = pedsQueryable.Select(ped => ped.Value).ToList();
        var page = new Page<CommonFPed>
        {
            Items = peds,
            CurrentPage = 1,
            ItemCount = peds.Count,
            PageCount = 1,
            PageSize = peds.Count,
        };

        networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
    }
}
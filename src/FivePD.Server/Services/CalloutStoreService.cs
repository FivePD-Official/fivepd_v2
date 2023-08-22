using System.Collections.Generic;
using System.Linq;
using FivePD.Server.API;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

#pragma warning disable

public class CalloutStoreService : ICalloutStoreService, IInternalCalloutStoreService
{
    private readonly List<CalloutHolder> _activeCallouts = new();

    public List<Callout> ActiveCallouts => this._activeCallouts.Select(x => x.Callout).ToList();

    List<CalloutHolder> IInternalCalloutStoreService.ActiveCallouts => this._activeCallouts;
}
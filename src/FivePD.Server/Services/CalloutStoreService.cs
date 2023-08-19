using System.Collections.Generic;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

#pragma warning disable

public class CalloutStoreService : ICalloutStoreService, IInternalCalloutStoreService
{
    private readonly List<CalloutHolder> _activeCallouts = new();

    List<CalloutHolder> IInternalCalloutStoreService.ActiveCallouts => this._activeCallouts;
}
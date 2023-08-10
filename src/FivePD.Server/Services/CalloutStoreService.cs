using System.Collections.Generic;
using System.Threading;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

#pragma warning disable

public class CalloutStoreService : ICalloutStoreService, IInternalCalloutStoreService
{
    private readonly List<CalloutType> _calloutTypes = new();

    private readonly List<CalloutHolder> _activeCallouts = new();

    List<CalloutType> IInternalCalloutStoreService.CalloutTypes => this._calloutTypes;

    List<CalloutHolder> IInternalCalloutStoreService.ActiveCallouts => this._activeCallouts;
}
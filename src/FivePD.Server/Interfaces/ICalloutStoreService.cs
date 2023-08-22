using System.Collections.Generic;
using System.Collections.ObjectModel;
using FivePD.Server.API;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface ICalloutStoreService
{
    public List<Callout> ActiveCallouts { get; }
}

internal interface IInternalCalloutStoreService
{
    public List<CalloutHolder> ActiveCallouts { get; }
}
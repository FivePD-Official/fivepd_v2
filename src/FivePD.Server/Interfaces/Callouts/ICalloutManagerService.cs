using System;
using System.Diagnostics;
using FivePD.Server.API;

namespace FivePD.Server.Interfaces.Callouts;

/// <summary>
/// Responsible for managing callouts.
/// </summary>
public interface ICalloutManagerService
{
}

internal sealed record InternalCallout(Callout Callout, CalloutSetupProperties Properties, Type Type, Stopwatch Timer);
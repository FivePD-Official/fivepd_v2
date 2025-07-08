using System.Collections.Generic;
using FivePD.Server.Models;
using FivePD.Server.Models.AssistanceServiceModels;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IAssistanceServiceConfigService
{
    /// <summary>
    /// Registers an assistance service of a given type with its options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <typeparam name="T">The type.</typeparam>
    public void Register<T>(AssistanceServiceOptions options)
        where T : AssistanceService;

    /// <summary>
    /// Gets all registered assistance services.
    /// </summary>
    /// <returns>A list of tuples with the type of the assistance service and its options.</returns>
    public IReadOnlyList<AvailableAssistanceService> AvailableServices { get; }
}
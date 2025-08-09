using System;
using FivePD.Server.Models.AssistanceServiceModels;

namespace FivePD.Server.Models;

/// <summary>
/// Represents an available assistance service.
/// </summary>
public sealed record AvailableAssistanceService
{
    /// <summary>
    /// Gets or sets the type of the assistance service.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Gets or sets the options for the assistance service.
    /// </summary>
    public AssistanceServiceOptions Options { get; set; }

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// Gets or sets the name of the assembly.
    /// </summary>
    public string AssemblyName { get; set; }
}
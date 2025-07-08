using System;
using FivePD.Server.Models.AssistanceServiceModels;

namespace FivePD.Server.Models;

#pragma warning disable

public sealed record AvailableAssistanceService
{
    public Type Type { get; set; }

    public AssistanceServiceOptions Options { get; set; }

    public string TypeName { get; set; }

    public string AssemblyName { get; set; }
}
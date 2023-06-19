using System;
using CitizenFX.Core;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Models.AssistanceServiceModels;

namespace FivePD.Gamemode.Server.Interfaces;

#pragma warning disable

public interface IAssistanceServiceManagerService
{
    public Guid Call<T>(FPlayer requestedBy)
        where T : AssistanceService;

    public void Cancel(Guid guid);
}
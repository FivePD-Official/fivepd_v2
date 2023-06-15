using System;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Models.AssistanceServiceModels;

namespace FivePD.Gamemode.Server.Interfaces;

#pragma warning disable

public interface IAssistanceServiceManagerService
{
    public Guid Call<T>(Vector3 location)
        where T : AssistanceService;

    public void Cancel(Guid guid);
}
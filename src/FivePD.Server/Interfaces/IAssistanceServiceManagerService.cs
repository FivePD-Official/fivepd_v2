using System;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Models.AssistanceServiceModels;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IAssistanceServiceManagerService
{
    public Guid Call<T>(FPlayer requestedBy, Action onCanceled = null)
        where T : AssistanceService;

    public void Cancel(ref Guid guid);
}
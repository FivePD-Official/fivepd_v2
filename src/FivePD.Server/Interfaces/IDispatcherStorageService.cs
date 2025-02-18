using System.Collections.Generic;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface IDispatcherStorageService
{
    public List<FPlayer> Dispatchers();
}
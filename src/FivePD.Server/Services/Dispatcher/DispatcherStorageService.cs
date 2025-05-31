using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces.Dispatcher;

namespace FivePD.Server.Services.Dispatcher;

#pragma warning disable

[Obfuscate]
public class DispatcherStorageService : IDispatcherStorageService
{
    private readonly PlayerList _playerList;
    
    public DispatcherStorageService(PlayerList playerList)
    {
        this._playerList = playerList;
    }
    
    public List<FPlayer> Dispatchers()
    {
        return this._playerList
            .Select(FPlayer.FromPlayer)
            .Where(p => p.Dispatcher.Value)
            .ToList();
    }
}
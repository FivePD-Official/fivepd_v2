using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

#pragma warning disable

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
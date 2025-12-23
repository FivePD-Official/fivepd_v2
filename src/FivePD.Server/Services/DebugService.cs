using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.DTO;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class DebugService : IDebugService
{
    private readonly INetworkEventService _networkCallbackDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugService"/> class.
    /// </summary>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    public DebugService(INetworkEventService networkEventService)
    {
        this._networkCallbackDelegate = networkEventService;
    }

    [UIEvent(Events.Debug.Information)]
    private void DebugInfoUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var fPlayer = FPlayer.FromPlayer(player);
        var user = fPlayer.User;

        var response = new DebugInfoDTO
        {
            CanAccessFivePD = user.AccessFivePD,
            IsInDepartment = user.Department is not null,
            IsInDivision = user.Division is not null,
            IsOnDuty = fPlayer.IsOnDuty,
            PreferredLanguage = user.PreferredLanguage,
        };

        this._networkCallbackDelegate.TriggerEventSerialized(player, guid, response);
    }
}
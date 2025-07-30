using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.DTO;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class NetworkEventService : INetworkEventService
{
    public void TriggerEvent(Player player, string guid, params object[] args)
    {
        player.TriggerEvent($"{Events.NetworkEvent.ClientToServer}:{guid}", args);
    }

    public void TriggerEventSerialized(Player player, string guid, object obj)
    {
        this.TriggerEvent(player, guid, Converter.SerializeObjectWithoutReferenceLoop(obj));
    }

    public void TriggerEventWithResponse(Player player, string guid, bool status, string title, string description, Dictionary<string, string> @params = null)
    {
        var response = new ActionResponse
        {
            Status = status,
            Title = title,
            Description = description,
        };

        if (@params != null)
        {
            response.Params = @params;
        }

        this.TriggerEventSerialized(player, guid, response);
    }

    public void TriggerEventWithUnexpectedError(Player player, string guid)
    {
        this.TriggerEventWithResponse(player, guid, false, "UI.ERROR.UNEXPECTED", "UI.ERROR.DATABASE_ACCESS.TITLE");
    }
}
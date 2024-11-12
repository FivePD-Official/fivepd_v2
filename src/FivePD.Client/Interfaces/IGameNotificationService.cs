using FivePD.Common.Models;

namespace FivePD.Client.Interfaces;

#pragma warning disable

internal interface IGameNotificationService
{
    public void Notify(GameNotificationOptions options);
}
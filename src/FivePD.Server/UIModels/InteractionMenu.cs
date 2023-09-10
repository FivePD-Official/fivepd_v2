using System.Collections.Generic;
using FivePD.Common;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.UIModels.MenuModels;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.UIModels;

/// <inheritdoc />
public sealed class InteractionMenu : Menu
{
    private readonly INotificationService _notificationService;
    private readonly IEntityService _entityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionMenu"/> class.
    /// </summary>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    /// <param name="entityService">The <see cref="IEntityService"/> to use.</param>
    public InteractionMenu(INotificationService notificationService, IEntityService entityService)
        : base(Controls.InteractionMenu.Id, Controls.InteractionMenu.Title, Controls.InteractionMenu.Key, true)
    {
        this._notificationService = notificationService;
        this._entityService = entityService;

        this.Items = this.CreateItems();
    }

    /// <summary>
    /// Gets the pat down item.
    /// </summary>
    public MenuButtonItem PatDown { get; private set; }

    private List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.PatDown = new MenuButtonItem("Pat down the ped", "asd");
        this.PatDown.OnSelect += player =>
        {
            async void RunAsync()
            {
                #pragma warning disable
                // var playerPed = FPed.FromFPlayer(player);
                // playerPed.Task.PlayAnimation();

                var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
                if (fPed is null)
                {
                    return;
                }

                var itemList = new GtaStyleText()
                    .Blue("Items: ")
                    .Reset();
                for (var i = 0; i < fPed.Items.Count; i++)
                {
                    if (fPed.Items[i].Illegal)
                    {
                        itemList.Red(fPed.Items[i].Title);
                    }
                    else
                    {
                        itemList.Append(fPed.Items[i].Title);
                    }

                    if (i < fPed.Items.Count - 1)
                    {
                        itemList.Append(", ");
                    }
                }

                // playerPed.Task.ClearAnimation();
                this._notificationService.ShowNotification(player, itemList.Build());
            }

            RunAsync();
        };

        items.Add(this.PatDown);

        return items;
    }
}
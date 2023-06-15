using System.Collections.Generic;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.UIModels;

#pragma warning disable

public sealed class AssistanceMenu : Menu
{
    private readonly ICalloutManagerService _calloutManagerService;

    public AssistanceMenu(ICalloutManagerService calloutManagerService)
        : base(Controls.AssistanceMenu.Id, Controls.AssistanceMenu.Title, Controls.AssistanceMenu.Key, true)
    {
        this._calloutManagerService = calloutManagerService;
        this.Items = this.CreateItems();
    }
    
    public MenuButtonItem CompleteIncident { get; private set; }

    private List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.CompleteIncident = new MenuButtonItem("Complete incident", string.Empty);
        this.CompleteIncident.OnSelect += player =>
        {
            this._calloutManagerService.CompleteCalloutOfPlayer(player);
        };
        items.Add(this.CompleteIncident);
        
        return items;
    }
}
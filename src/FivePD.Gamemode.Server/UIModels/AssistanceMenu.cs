using System.Collections.Generic;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models.AssistanceServiceModels;

namespace FivePD.Gamemode.Server.UIModels;

#pragma warning disable

public sealed class AssistanceMenu : Menu
{
    private readonly ICalloutManagerService _calloutManagerService;
    private readonly IAssistanceServiceManagerService _assistanceService;

    public AssistanceMenu(ICalloutManagerService calloutManagerService, IAssistanceServiceManagerService assistanceServiceManagerService)
        : base(Controls.AssistanceMenu.Id, Controls.AssistanceMenu.Title, Controls.AssistanceMenu.Key, true)
    {
        this._calloutManagerService = calloutManagerService;
        this._assistanceService = assistanceServiceManagerService;
        this.Items = this.CreateItems();
    }
    
    public MenuButtonItem CompleteIncident { get; private set; }

    public MenuButtonItem CallAmbulance { get; private set; }

    private List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.CompleteIncident = new MenuButtonItem("Complete incident", string.Empty);
        this.CompleteIncident.OnSelect += player =>
        {
            this._calloutManagerService.CompleteCalloutOfPlayer(player);
        };
        items.Add(this.CompleteIncident);
        
        this.CallAmbulance = new MenuButtonItem("Call ambulance", string.Empty);
        this.CallAmbulance.OnSelect += player =>
        {
            var position = player.GetUnderlyingPlayer().Character.Position;
            position.X += 15f;
            this._assistanceService.Call<Ambulance>(position);
        };
        items.Add(this.CallAmbulance);
        
        return items;
    }
}
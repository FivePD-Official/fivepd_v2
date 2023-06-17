using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models.AssistanceServiceModels;
using FivePD.Gamemode.Server.UIModels.MenuModels;

namespace FivePD.Gamemode.Server.UIModels;

#pragma warning disable

public sealed class DispatchMenu : Menu
{
    private readonly ICalloutManagerService _calloutManagerService;
    private readonly IAssistanceServiceManagerService _assistanceService;

    public DispatchMenu(ICalloutManagerService calloutManagerService, IAssistanceServiceManagerService assistanceServiceManagerService)
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
            // var position = player.GetUnderlyingPlayer().Character.Position;
            // position.X += 15f;
            this._assistanceService.Call<Ambulance>(new Vector3(704.09f, -117.47f, 52.83f), player.GetUnderlyingPlayer().Character.Position);
            // this._assistanceService.Call<Ambulance>(new Vector3(735.28f, 1289.64f, 360.3f));
        };
        items.Add(this.CallAmbulance);
        
        return items;
    }
}
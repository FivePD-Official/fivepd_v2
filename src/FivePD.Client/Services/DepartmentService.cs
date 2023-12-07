using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IDepartmentService" />
public class DepartmentService : IDepartmentService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public DepartmentService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.GetAll);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.GetUsers);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.GetRank);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.GetRanks);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.GetDivision);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.GetDivisions);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.DeleteDivision);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.EditDivision);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.CreateDivision);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.DeleteRank);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.EditRank);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.CreateRank);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.Join);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.Leave);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.RemoveUser);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.Edit);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.Delete);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.Create);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.CheckIfDepartmentNameTaken);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.GetAllSimpleWithArea);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Department.UpdateAreas);
    }
}
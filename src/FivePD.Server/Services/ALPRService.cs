using System;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class AlprService : IAlprService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlprService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">Event handler dictionary.</param>
    public AlprService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary[Events.Vehicle.FetchData] += OnFetchData;
    }

    private void OnFetchData([FromSource] Player source, int netId)
    {
        var fVehicle = FVehicle.FromNetId(netId);
        var ownerPed = FPed.FromNetId(fVehicle.OwnerNetworkId);
        if (ownerPed.Warrants.Count > 0)
        {
            string flaggedWarrant = ownerPed.Warrants.First();

            string insurance = fVehicle.GetDocument(VehicleDocumentType.Insurance).Status.ToString() == VehicleDocumentStatus.Valid.ToString()
                ? $"~g~Valid"
                : $"~r~{fVehicle.GetDocument(VehicleDocumentType.Insurance).Status.ToString()}";
            string registration = fVehicle.GetDocument(VehicleDocumentType.Registration).Status.ToString() == VehicleDocumentStatus.Valid.ToString()
                ? $"~g~Valid"
                : $"~r~{fVehicle.GetDocument(VehicleDocumentType.Registration).Status.ToString()}";

            string content = $@"~f~[ALPR]~s~
~y~Vehicle brand:~s~ {fVehicle.Brand}
~y~Vehicle color:~s~ {fVehicle.Color}
~y~License plate:~s~ {fVehicle.LicensePlate}
~y~Owner:~s~ {ownerPed.Firstname} {ownerPed.Lastname}
~y~Flag:~s~  {flaggedWarrant}
~y~Insurance:~s~ {insurance}
~y~Registration:~s~ {registration}
";
            BaseScript.TriggerClientEvent(source, Events.Vehicle.FetchData, ownerPed.Warrants.Count > 0, content);
            return;
        }

        BaseScript.TriggerClientEvent(source, Events.Vehicle.FetchData, ownerPed.Warrants.Count > 0);
    }
}
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class CalloutDetailService : ICalloutDetailService
{
    private readonly DatabaseContext _databaseContext;
    private readonly INetworkEventService _networkEventService;

    public CalloutDetailService(DatabaseContext databaseContext, INetworkEventService networkEventService)
    {
        this._databaseContext = databaseContext;
        this._networkEventService = networkEventService;
    }

    [UIEvent(Events.UI.CalloutDetails.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CalloutDetailsRequest>(serializedRequest);
        var page = this._databaseContext.CalloutDetails
            .AsNoTracking()
            .Paginate(request.Page, new Filters<CalloutDetails>());
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.CalloutDetails.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var calloutDetails = this._databaseContext.CalloutDetails
            .AsNoTracking()
            .Include(x => x.CalloutDetailsDepartments)
            .FirstOrDefault(x => x.Id == request.Id);
        this._networkEventService.TriggerEventSerialized(player, guid, calloutDetails);
    }

    [UIEvent(Events.UI.CalloutDetails.UpdateRuleset)]
    private void UpdateRulesetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CalloutDetailsUpdateRulesetRequest>(serializedRequest);
        var calloutDetails = this._databaseContext.CalloutDetails
            .Include(x => x.CalloutDetailsDepartments)
            .FirstOrDefault(x => x.Id == request.Id);
        if (calloutDetails is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        calloutDetails.RulesetType = request.RulesetType;

        foreach (var calloutDetailsDepartment in calloutDetails.CalloutDetailsDepartments)
        {
            if (!request.Departments.Contains(calloutDetailsDepartment.DepartmentId))
            {
                this._databaseContext.CalloutDetailsDepartments.Remove(calloutDetailsDepartment);
            }
        }

        foreach (var a in request.Departments)
        {
            if (calloutDetails.CalloutDetailsDepartments.All(x => x.DepartmentId != a))
            {
                this._databaseContext.CalloutDetailsDepartments.Add(new CalloutDetailsDepartment
                {
                    CalloutDetailsId = calloutDetails.Id,
                    DepartmentId = a
                });
            }
        }

        this._databaseContext.SaveChanges();
        // TODO: This needs to be localized
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "Successful update", "Ruleset has been updated.");
    }
}
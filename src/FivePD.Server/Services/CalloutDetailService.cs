using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.Requests;
using FivePD.Server.Repositories;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class CalloutDetailService : ICalloutDetailService
{
    private readonly CalloutDetailsRepository _calloutDetailsRepository;
    private readonly DepartmentRepository _departmentRepository;
    private readonly INetworkEventService _networkEventService;

    public CalloutDetailService(
        CalloutDetailsRepository calloutDetailsRepository,
        DepartmentRepository departmentRepository,
        INetworkEventService networkEventService)
    {
        this._calloutDetailsRepository = calloutDetailsRepository;
        this._departmentRepository = departmentRepository;
        this._networkEventService = networkEventService;
    }

    [UIEvent(Events.UI.CalloutDetails.GetAllPageable)]
    private void GetAllPageableUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CalloutDetailsRequest>(serializedRequest);
        var page = this._calloutDetailsRepository.Paginate(request.Page);
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.CalloutDetails.Get)]
    private void GetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<IdRequest>(serializedRequest);
        var calloutDetails = this._calloutDetailsRepository.FindByIdWithDynamicRelated(
            request.Id,
            new CalloutDetailsRepository.CalloutDetailsLoadOptions { IncludeCalloutDetailsDepartments = true });
        this._networkEventService.TriggerEventSerialized(player, guid, calloutDetails);
    }

    [UIEvent(Events.UI.CalloutDetails.UpdateRuleset)]
    private void UpdateRulesetUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<CalloutDetailsUpdateRulesetRequest>(serializedRequest);
        var calloutDetails = this._calloutDetailsRepository.FindByIdWithDynamicRelated(
            request.Id,
            new CalloutDetailsRepository.CalloutDetailsLoadOptions { IncludeCalloutDetailsDepartments = true });
        if (calloutDetails is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._calloutDetailsRepository.Update(calloutDetails, x => { x.RulesetType = request.RulesetType; });

        foreach (var calloutDetailsDepartment in calloutDetails.CalloutDetailsDepartments)
        {
            if (!request.Departments.Contains(calloutDetailsDepartment.DepartmentId))
            {
                var department = this._departmentRepository.FindById(calloutDetailsDepartment.DepartmentId);
                this._calloutDetailsRepository.RemoveDepartment(calloutDetails, department);
            }
        }

        foreach (var id in request.Departments)
        {
            if (calloutDetails.CalloutDetailsDepartments.All(x => x.DepartmentId != id))
            {
                var department = this._departmentRepository.FindById(id);
                this._calloutDetailsRepository.AttachDepartment(calloutDetails, department);
            }
        }

        // TODO: This needs to be localized
        this._networkEventService.TriggerEventWithResponse(player, guid, true, "Successful update", "Ruleset has been updated.");
    }
}
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

namespace FivePD.Server.Services;

#pragma warning disable

[Obfuscate]
public class CalloutDetailService : ICalloutDetailService
{
    private readonly DatabaseContext _databaseContext;

    public CalloutDetailService(DatabaseContext databaseContext)
    {
        this._databaseContext = databaseContext;
    }

    [UIEvent(Events.UI.CalloutDetails.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<CalloutDetailsRequest>();
        var page = this._databaseContext.CalloutDetails
            .AsNoTracking()
            .Paginate(request.Page, new Filters<CalloutDetails>());
        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.CalloutDetails.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var calloutDetails = this._databaseContext.CalloutDetails
            .AsNoTracking()
            .Include(x => x.CalloutDetailsDepartments)
            .FirstOrDefault(x => x.Id == request.Id);
        networkCallbackDelegate.InvokeWithJson(calloutDetails);
    }

    [UIEvent(Events.UI.CalloutDetails.UpdateRuleset)]
    private void UpdateRulesetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<CalloutDetailsUpdateRulesetRequest>();
        var calloutDetails = this._databaseContext.CalloutDetails
            .Include(x => x.CalloutDetailsDepartments)
            .FirstOrDefault(x => x.Id == request.Id);
        if (calloutDetails is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
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
        networkCallbackDelegate.InvokeWithResponse(true, "Successful update", "Ruleset has been updated.");
    }
}
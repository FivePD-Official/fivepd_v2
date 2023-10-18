using System.Collections.Generic;

namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal struct CalloutDetailsUpdateRulesetRequest
{
    public int Id { get; set; }
    
    public CalloutDetailsRulesetType RulesetType { get; set; }
    
    public List<int> Departments { get; set; }
}
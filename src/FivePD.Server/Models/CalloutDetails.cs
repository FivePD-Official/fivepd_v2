using System.Collections.Generic;

namespace FivePD.Server.Models;

#pragma warning disable

public class CalloutDetails
{
    public int Id { get; set; }
    
    public bool Active { get; set; }
    
    public string TypeName { get; set; }
    
    public string AssemblyName { get; set; }

    public CalloutDetailsRulesetType RulesetType { get; set; }

    public IList<CalloutDetailsDepartment> CalloutDetailsDepartments { get; set; }
}
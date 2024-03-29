using System.ComponentModel.DataAnnotations.Schema;

namespace FivePD.Server.Models;

#pragma warning disable

[Table("fivepd_callout_details_departments")]
public class CalloutDetailsDepartment
{
    public int CalloutDetailsId { get; set; }

    public CalloutDetails CalloutDetails { get; set; }

    public int DepartmentId { get; set; }

    public Department Department { get; set; }
}
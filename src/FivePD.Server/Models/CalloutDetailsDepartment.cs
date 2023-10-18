namespace FivePD.Server.Models;

#pragma warning disable

public class CalloutDetailsDepartment
{
    public int CalloutDetailsId { get; set; }

    public CalloutDetails CalloutDetails { get; set; }

    public int DepartmentId { get; set; }

    public Department Department { get; set; }
}
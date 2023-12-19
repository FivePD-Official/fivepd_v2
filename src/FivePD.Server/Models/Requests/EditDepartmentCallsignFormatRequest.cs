using System.Collections.Generic;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditDepartmentCallsignFormatRequest()
{
    public int DepartmentId { get; set; }

    public List<CallsignFormatItem> CallsignFormat { get; set; }
}
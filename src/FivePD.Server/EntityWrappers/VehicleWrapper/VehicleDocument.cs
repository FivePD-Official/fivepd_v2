using System;

namespace FivePD.Server.EntityWrappers.VehicleWrapper;

#pragma warning disable

public class VehicleDocument
{
    public VehicleDocumentStatus Status { get; init; }

    public VehicleDocumentType Type { get; init; }
    
    public DateTime IssueDate { get; init; }

    public DateTime ExpirationDate { get; init; }
}
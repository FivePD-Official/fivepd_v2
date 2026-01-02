using System.Collections.Generic;
using FivePD.Server.Attributes;

namespace FivePD.Server.Models.DTO;

/// <inheritdoc />
[Obfuscate]
internal class DepartmentResponse : Department
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentResponse"/> class.
    /// </summary>
    /// <param name="department">The <see cref="Department"/> to create this instance from.</param>
    /// <param name="userCount">The number of users.</param>
    public DepartmentResponse(Department department, int userCount)
    {
        this.Id = department.Id;
        this.Name = department.Name;
        this.Callsign = department.Callsign;
        this.CallsignFormat = department.CallsignFormat;
        this.MaxUsers = department.MaxUsers;
        this.Divisions = department.Divisions;
        this.DepartmentRanks = new List<DepartmentRank>();
        this.Users = new List<User>();
        this.UserCount = userCount;
    }

    /// <summary>
    /// Gets the number of users.
    /// </summary>
    public int UserCount { get; private set; }
}
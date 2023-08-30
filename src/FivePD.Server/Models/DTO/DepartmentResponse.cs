using System.Collections.Generic;
using System.Linq;

namespace FivePD.Server.Models.DTO;

/// <inheritdoc />
internal class DepartmentResponse : Department
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentResponse"/> class.
    /// </summary>
    /// <param name="department">The <see cref="Department"/> to create this instance from.</param>
    public DepartmentResponse(Department department)
    {
        this.Id = department.Id;
        this.Name = department.Name;
        this.Callsign = department.Callsign;
        this.CallsignFormat = department.CallsignFormat;
        this.MaxUsers = department.MaxUsers;
        this.Divisions = department.Divisions;
        this.Ranks = new List<Rank>();
        this.Users = new List<User>();

        this.UserCount = department.Users.Count();
    }

    /// <summary>
    /// Gets the number of users.
    /// </summary>
    public int UserCount { get; private set; }
}
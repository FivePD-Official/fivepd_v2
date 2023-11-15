namespace FivePD.Server.Models;

/// <summary>
/// Represents the 'department_area_coordinates' table.
/// </summary>
public class DepartmentAreaCoordinate
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    public Department Department { get; set; }

    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public float Y { get; set; }
}
using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Responsible for managing <see cref="Department"/> objects.
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// Fires when a department has been created.
    /// </summary>
    public event EventHandler<Department> DepartmentCreated;

    /// <summary>
    /// Fires when a department has been edited.
    /// </summary>
    public event EventHandler<Department> DepartmentEdited;

    /// <summary>
    /// Fires when a department has been deleted.
    /// </summary>
    public event EventHandler<Department> DepartmentDeleted;

    /// <summary>
    /// Fires when a rank has been created.
    /// </summary>
    public event EventHandler<Rank> RankCreated;

    /// <summary>
    /// Fires when a rank has been edited.
    /// </summary>
    public event EventHandler<Rank> RankEdited;

    /// <summary>
    /// Fires when a rank has been deleted.
    /// </summary>
    public event EventHandler<Rank> RankDeleted;

    /// <summary>
    /// Fires when a division has been created.
    /// </summary>
    public event EventHandler<Division> DivisionCreated;

    /// <summary>
    /// Fires when a division has been edited.
    /// </summary>
    public event EventHandler<Division> DivisionEdited;

    /// <summary>
    /// Fires when a division has been deleted.
    /// </summary>
    public event EventHandler<Division> DivisionDeleted;

    /// <summary>
    /// Fires when a player joins a department or has been added to one with the API.
    /// </summary>
    public event EventHandler<PlayerDepartmentChangeEventArgs> PlayerJoinedDepartment;

    /// <summary>
    /// Fires when a player leaves a department or has been removed from one with the API.
    /// </summary>
    public event EventHandler<PlayerDepartmentChangeEventArgs> PlayerLeftDepartment;

    /// <summary>
    /// Inserts a <see cref="Database"/> into the 'departments' table.
    /// </summary>
    /// <param name="department">The department to insert.</param>
    /// <exception cref="ArgumentException">The provided department or its name is null.</exception>
    /// <exception cref="DepartmentExistsWithNameException">A department already exists with the same name.</exception>
    /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
    /// <returns>The created department object with change tracking.</returns>
    public Department CreateDepartment(Department department);

    /// <summary>
    /// Removes a <see cref="Database"/> from the 'departments' table.
    /// </summary>
    /// <param name="department">The department to remove.</param>
    /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
    public void RemoveDepartment(Department department);

    /// <summary>
    /// Selects and returns all departments.
    /// </summary>
    /// <param name="includeRanks">Specifies whether to include the departments' ranks in the query or not.</param>
    /// <param name="includeDivisions">Specifies whether to include the departments' divisions in the query or not.</param>
    /// <param name="includeUsers">Specifies whether to include the departments' users in the query or not.</param>
    /// <returns>A list of departments.</returns>
    public IEnumerable<Department> GetDepartments(bool includeRanks = false, bool includeDivisions = false, bool includeUsers = false);

    /// <summary>
    /// Finds the first department with the given identifier.
    /// </summary>
    /// <param name="id">The identifier to search for.</param>
    /// <param name="includeRanks">Specifies whether to include the department's ranks in the query or not.</param>
    /// <param name="includeDivisions">Specifies whether to include the department's divisions in the query or not.</param>
    /// <param name="includeUsers">Specifies whether to include the department's users in the query or not.</param>
    /// <returns>The found department.</returns>
    public Department GetDepartmentById(int id, bool includeRanks = false, bool includeDivisions = false, bool includeUsers = false);

    /// <summary>
    /// Saves all changes made to any <see cref="Department"/> object.
    /// </summary>
    /// <returns>If the operation was successful.</returns>
    public bool SaveChanges();

    /// <summary>
    /// Adds the given player to the given department.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="department">The department to which the user will be added.</param>
    /// <exception cref="ArgumentException">Any of the provided arguments is null.</exception>
    /// <exception cref="DepartmentFullException">The provided department is full.</exception>
    /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
    public void AddPlayerToDepartment(User user, Department department);

    /// <summary>
    /// Adds the given player to the given department.
    /// </summary>
    /// <param name="player">The user to update.</param>
    /// <param name="department">The department to which the user will be added.</param>
    /// <exception cref="ArgumentException">Any of the provided arguments is null.</exception>
    /// <exception cref="DepartmentFullException">The provided department is full.</exception>
    /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
    public void AddPlayerToDepartment(Player player, Department department);

    /// <summary>
    /// The given player will be removed from their department.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <exception cref="ArgumentException">Any of the provided arguments is null.</exception>
    /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
    public void RemovePlayerFromDepartment(User user);

    /// <summary>
    /// The given player will be removed from their department.
    /// </summary>
    /// <param name="player">The user to update.</param>
    /// <exception cref="ArgumentException">Any of the provided arguments is null.</exception>
    /// <exception cref="DatabaseUsageException">Database error during connection or usage at runtime.</exception>
    public void RemovePlayerFromDepartment(Player player);
}

// TODO: should be moved to its own file somewhere

/// <summary>
/// Provides additional information regarding a player's
/// department change, both when they join or leave one.
/// </summary>
public class PlayerDepartmentChangeEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the player who is affected by this change.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Gets or sets the department which the player has joined or left.
    /// </summary>
    public Department Department { get; set; }
}
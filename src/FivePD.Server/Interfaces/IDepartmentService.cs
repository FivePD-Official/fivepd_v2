using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Exceptions;
using FivePD.Server.Models;
using FivePD.Server.Models.Events;

namespace FivePD.Server.Interfaces;

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
    public event EventHandler<Department> DepartmentUpdated;

    /// <summary>
    /// Fires when a department has been deleted.
    /// </summary>
    public event EventHandler<Department> DepartmentDeleted;

    /// <summary>
    /// Fires when a division has been created.
    /// </summary>
    public event EventHandler<Division> DivisionCreated;

    /// <summary>
    /// Fires when a division has been edited.
    /// </summary>
    public event EventHandler<Division> DivisionUpdated;

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
    /// Inserts a <see cref="Department"/> into the 'departments' table.
    /// </summary>
    /// <param name="department">The department to insert.</param>
    /// <exception cref="ArgumentException">The provided department or its name is null.</exception>
    /// <exception cref="DepartmentExistsWithNameException">A department already exists with the same name.</exception>
    /// <returns>The created department object with change tracking.</returns>
    public Department CreateDepartment(Department department);

    /// <summary>
    /// Updates the given department.
    /// </summary>
    /// <param name="department">The department to update.</param>
    /// <exception cref="ArgumentException">The provided department or its name is null.</exception>
    /// <exception cref="DepartmentExistsWithNameException">A department already exists with the same name.</exception>
    /// <returns>The updated department object with change tracking.</returns>
    public Department UpdateDepartment(Department department);

    /// <summary>
    /// Deletes a <see cref="Department"/> from the 'departments' table.
    /// </summary>
    /// <param name="department">The department to remove.</param>
    public void DeleteDepartment(Department department);

    /// <summary>
    /// Inserts a <see cref="Division"/> into the 'divisions' table.
    /// </summary>
    /// <param name="division">The division to insert.</param>
    /// <returns>The created division object with change tracking.</returns>
    public Division CreateDivision(Division division);

    /// <summary>
    /// Updates the given division.
    /// </summary>
    /// <param name="division">The division to update.</param>
    /// <returns>The updated division object with change tracking.</returns>
    public Division UpdateDivision(Division division);

    /// <summary>
    /// Deletes a <see cref="Division"/> from the 'departments' table.
    /// </summary>
    /// <param name="division">The department to remove.</param>
    public void DeleteDivision(Division division);

    /// <summary>
    /// Selects and returns all departments.
    /// </summary>
    /// <param name="options">The options to use when querying the database.</param>
    /// <returns>A list of departments.</returns>
    public IEnumerable<Department> GetDepartments(DepartmentQueryOptions options = default);

    /// <summary>
    /// Finds the first department with the given identifier.
    /// </summary>
    /// <param name="id">The identifier to search for.</param>
    /// <param name="options">The options to use when querying the database.</param>
    /// <returns>The found department.</returns>
    public Department GetDepartmentById(int id, DepartmentQueryOptions options = default);

    /// <summary>
    /// Adds the given player to the given department.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="department">The department to which the user will be added.</param>
    /// <exception cref="ArgumentException">Any of the provided arguments is null.</exception>
    /// <exception cref="DepartmentFullException">The provided department is full.</exception>
    public void AddPlayerToDepartment(User user, Department department);

    /// <summary>
    /// Adds the given player to the given department.
    /// </summary>
    /// <param name="player">The user to update.</param>
    /// <param name="department">The department to which the user will be added.</param>
    /// <exception cref="ArgumentException">Any of the provided arguments is null.</exception>
    /// <exception cref="DepartmentFullException">The provided department is full.</exception>
    public void AddPlayerToDepartment(Player player, Department department);

    /// <summary>
    /// The given player will be removed from their department.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <exception cref="ArgumentException">Any of the provided arguments is null.</exception>
    public void RemovePlayerFromDepartment(User user);

    /// <summary>
    /// The given player will be removed from their department.
    /// </summary>
    /// <param name="player">The user to update.</param>
    /// <exception cref="ArgumentException">Any of the provided arguments is null.</exception>
    public void RemovePlayerFromDepartment(Player player);

    /// <summary>
    /// The given player will be removed from their department.
    /// </summary>
    /// <param name="player">The user to update.</param>
    /// <exception cref="ArgumentException">Any of the provided arguments is null.</exception>
    public void RemovePlayerFromDepartment(FPlayer player);

    /// <summary>
    /// Gets the department of the given player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The department of the player.</returns>
    public Department GetPlayerDepartment(Player player);

    /// <summary>
    /// Gets the department of the given player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The department of the player.</returns>
    public Department GetPlayerDepartment(FPlayer player);
}

/// <summary>
/// Provides options for querying departments.
/// </summary>
/// <param name="IncludeRanks">Specifies whether to include ranks in the query.</param>
/// <param name="IncludeDivisions">Specifies whether to include divisions in the query.</param>
/// <param name="IncludeUsers">Specifies whether to include users in the query.</param>
#pragma warning disable SA1313
public readonly record struct DepartmentQueryOptions(bool IncludeRanks, bool IncludeDivisions, bool IncludeUsers);
#pragma warning restore SA1313

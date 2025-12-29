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

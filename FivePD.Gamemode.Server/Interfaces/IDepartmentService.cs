// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.Interfaces
{
    /// <summary>
    /// Responsible for managing <see cref="Department"/> objects.
    /// </summary>
    public interface IDepartmentService
    {
        /// <summary>
        /// Inserts a <see cref="Database"/> into the 'departments' table.
        /// </summary>
        /// <param name="department">The department to insert.</param>
        /// <returns>If the operation was successful.</returns>
        public bool CreateDepartment(Department department);

        /// <summary>
        /// Removes a <see cref="Database"/> from the 'departments' table.
        /// </summary>
        /// <param name="department">The department to remove.</param>
        /// <returns>If the operation was successful.</returns>
        public bool RemoveDepartment(Department department);

        /// <summary>
        /// Selects and returns all departments.
        /// </summary>
        /// <returns>A list of departments.</returns>
        public IEnumerable<Department> GetDepartments();

        /// <summary>
        /// Finds the first department with the given identifier.
        /// </summary>
        /// <param name="id">The identifier to search for.</param>
        /// <returns>The found department.</returns>
        public Department GetDepartmentById(int id);

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
        /// <returns>If the operation was successful.</returns>
        public bool AddPlayerToDepartment(User user, Department department);

        /// <summary>
        /// Adds the given player to the given department.
        /// </summary>
        /// <param name="player">The user to update.</param>
        /// <param name="department">The department to which the user will be added.</param>
        /// <returns>If the operation was successful.</returns>
        public bool AddPlayerToDepartment(Player player, Department department);

        /// <summary>
        /// The given player will be removed from their department.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>If the operation was successful.</returns>
        public bool RemovePlayerFromDepartment(User user);

        /// <summary>
        /// The given player will be removed from their department.
        /// </summary>
        /// <param name="player">The user to update.</param>
        /// <returns>If the operation was successful.</returns>
        public bool RemovePlayerFromDepartment(Player player);
    }
}
// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;

namespace FivePD.Gamemode.Server.Attributes
{
    /// <summary>
    /// asdasd.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LooseDataEntityModelAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LooseDataEntityModelAttribute"/> class.
        /// </summary>
        /// <param name="className">The id.</param>
        public LooseDataEntityModelAttribute(string className)
        {
            this.ClassName = className;
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public string ClassName { get; }
    }
}
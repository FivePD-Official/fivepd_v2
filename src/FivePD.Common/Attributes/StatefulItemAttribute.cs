// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;

namespace FivePD.Common.Attributes
{
    /// <summary>
    /// An attribute that declares a property
    /// as a stateful item in a container.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Property)]
    public class StatefulItemAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatefulItemAttribute"/> class.
        /// </summary>
        /// <param name="identifier">The unique identifier for this item.</param>
        /// <param name="defaultValue">The default value for the item if none is set.</param>
        public StatefulItemAttribute(string identifier, object defaultValue)
        {
            this.Identifier = identifier;
            this.DefaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the unique identifier for this item.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Gets the default value for the item.
        /// </summary>
        public object DefaultValue { get; }
    }
}
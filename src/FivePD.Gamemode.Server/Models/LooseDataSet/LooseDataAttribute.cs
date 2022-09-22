// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System.Collections.Generic;

namespace FivePD.Gamemode.Server.Models.LooseDataSet
{
    /// <summary>
    /// Represents a <see cref="LooseDataSet"/> attribute.
    /// </summary>
    public class LooseDataAttribute
    {
        /// <summary>
        /// Should be used to specify whether an attribute was created by an admin or not.
        /// </summary>
        public const string AddedByAdminAddonId = "admin";

        /// <summary>
        /// Defines the type of the attribute.
        /// Mainly used for displaying the correct input fields in the NUI.
        /// </summary>
        public enum LooseDataType
        {
            /// <summary>
            /// String.
            /// </summary>
            String,

            /// <summary>
            /// Integer.
            /// </summary>
            Integer,

            /// <summary>
            /// Bool.
            /// </summary>
            Bool,

            /// <summary>
            /// DateTime.
            /// </summary>
            DateTime,
        }

        /// <summary>
        /// Gets or sets the id of this attribute.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets which set this attribute belongs to.
        /// </summary>
        public LooseDataSet Set { get; set; }

        /// <summary>
        /// Gets or sets the data type of this attribute.
        /// </summary>
        public LooseDataType DataType { get; set; }

        /// <summary>
        /// Gets or sets the id of the addon which created this attribute.
        /// If it was created by an admin it should equal to <see cref="LooseDataAttribute.AddedByAdminAddonId"/>.
        /// </summary>
        public string AddonId { get; set; }

        /// <summary>
        /// Gets or sets the name of this attribute.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order of this attribute.
        /// Used for overriding the default order of grid columns.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute is required in forms or not.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute should be displayed in forms or not.
        /// </summary>
        public bool Readonly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute should be displayed in grids or not.
        /// </summary>
        public bool ShowInGrid { get; set; }

        /// <summary>
        /// Gets or sets the placeholder which will be shown in the attribute's input field.
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// Gets or sets the label which will be shown above the attribute's input field.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the help label which will be shown under the attribute's input field.
        /// </summary>
        public string HelpLabel { get; set; }

        /// <summary>
        /// Gets or sets the custom options of this attribute.
        /// </summary>
        public AttributeOptions Options { get; set; }

        /// <summary>
        /// Represents the possible options of an attribute.
        /// </summary>
        public class AttributeOptions
        {
            /// <summary>
            /// Gets or sets the items which will be displayed in dropdown fields.
            /// Only works if the <see cref="LooseDataAttribute.DataType"/> is set to <see cref="LooseDataType.Integer"/>.
            /// </summary>
            public Dictionary<int, string> Items { get; set; }
        }
    }
}
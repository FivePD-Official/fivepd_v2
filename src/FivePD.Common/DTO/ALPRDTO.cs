namespace FivePD.Common.DTO
{
    internal sealed record AlprDto
    {
        /// <summary>
        /// Class representing the ALPR (Automatic License Plate Recognition) DTO (Data Transfer Object).
        /// </summary>
        public record struct AlprDTO
        {
            /// <summary>
            /// Gets or sets a value indicating whether the license plate is wanted or not.
            /// </summary>
            /// <value>
            /// A <see cref="bool"/> value.
            /// </value>
            public bool WantedStatus { get; set; }

            /// <summary>
            /// Gets or sets the Data related to the license plate.
            /// </summary>
            /// <value>
            /// A <see cref="string"/> value.
            /// </value>
            public string Data { get; set; }
        }
    }
}
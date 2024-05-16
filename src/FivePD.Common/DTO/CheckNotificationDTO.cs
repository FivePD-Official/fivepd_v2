namespace FivePD.Common.DTO
{
    /// <summary>
    /// Gets or sets a value indicating whether class representing the ALPR (Automatic License Plate Recognition) DTO (Data Transfer Object).
    /// </summary>
    internal record struct CheckNotificationDTO
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
        public string NotificationContent { get; set; }
    }
}
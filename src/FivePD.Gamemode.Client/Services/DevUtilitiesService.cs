using CitizenFX.Core.UI;
using FivePD.Gamemode.Client.Interfaces;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class DevUtilitiesService : IDevUtilitiesService
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevUtilitiesService"/> class.
        /// </summary>
        /// <param name="notificationService">The
        /// <see cref="INotificationService"/> to use.</param>
        public DevUtilitiesService(INotificationService notificationService)
        {
            this._notificationService = notificationService;
        }

        /// <inheritdoc/>
        public void ShowDevelopmentMessage(string message)
        {
            this._notificationService.ShowNotificationWithTexture(
                "CHAR_DETONATEPHONE",
                "FivePD",
                "Development Message",
                message);
        }
    }
}
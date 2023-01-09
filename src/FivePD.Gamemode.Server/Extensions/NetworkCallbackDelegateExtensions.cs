using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Models.Responses;

namespace FivePD.Gamemode.Server.Extensions
{
    /// <summary>
    /// Holds extension methods for <see cref="NetworkCallbackDelegate"/>.
    /// </summary>
    public static class NetworkCallbackDelegateExtensions
    {
        /// <summary>
        /// Invokes the callback with a <see cref="ActionResponse"/> object.
        /// </summary>
        /// <param name="networkCallbackDelegate">The <see cref="NetworkCallbackDelegate"/> to invoke.</param>
        /// <param name="status">The status of the response.</param>
        /// <param name="title">The title of the response.</param>
        /// <param name="description">The description of the response.</param>
        /// <param name="params">The params of the response.</param>
        public static void InvokeWithResponse(this NetworkCallbackDelegate networkCallbackDelegate, bool status, string title, string description, Dictionary<string, string> @params = null)
        {
            var response = new ActionResponse
            {
                Status = status,
                Title = title,
                Description = description,
            };

            if (@params != null)
            {
                response.Params = @params;
            }

            networkCallbackDelegate.Invoke(response.ToString());
        }

        /// <summary>
        /// Invokes the callback with a false status and
        /// the error message Unexpected error.
        /// </summary>
        /// <param name="networkCallbackDelegate">The <see cref="NetworkCallbackDelegate"/> to invoke.</param>
        public static void InvokeWithUnexpectedError(this NetworkCallbackDelegate networkCallbackDelegate)
        {
            networkCallbackDelegate.InvokeWithResponse(false, "Nui.Errors.UnexpectedError", "Nui.Errors.DatabaseAccess");
        }
    }
}
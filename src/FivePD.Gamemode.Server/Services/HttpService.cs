using System;
using System.Net.Http;
using FivePD.Gamemode.Server.Interfaces;

#pragma warning disable S1075

namespace FivePD.Gamemode.Server.Services
{
    /// <inheritdoc />
    public class HttpService : IHttpService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpService"/> class.
        /// </summary>
        public HttpService()
        {
            this.Client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:1818"), // TODO: Use lambda uri for prod
            };
        }

        /// <inheritdoc />
        public HttpClient Client { get; }
    }
}
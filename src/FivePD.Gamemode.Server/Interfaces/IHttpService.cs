using System.Net.Http;

namespace FivePD.Gamemode.Server.Interfaces;

/// <summary>
/// Provides HTTP related utilities.
/// </summary>
public interface IHttpService
{
    /// <summary>
    /// Gets the HTTP client.
    /// </summary>
    HttpClient Client { get; }
}
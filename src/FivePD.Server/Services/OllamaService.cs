using System;
using System.Net.Http;
using System.Threading.Tasks;
using FivePD.Server.Interfaces;
using Ollama;

namespace FivePD.Server.Services;

/// <summary>
/// Implementation of <see cref="IOllamaService"/> that provides access to the Ollama API.
/// </summary>
public sealed class OllamaService : IOllamaService
{
    private readonly OllamaApiClient _client;

    private readonly IConfigService _configService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaService"/> class.
    /// </summary>
    /// <param name="configService">The configuration service used to obtain the Ollama model settings.</param>
    public OllamaService(IConfigService configService)
    {
        this._configService = configService;
        this._client = new OllamaApiClient(new HttpClient(), new Uri(this._configService.GeneralConfig.Ollama.Url));
    }

    /// <inheritdoc />
    public async Task<string> GetCompletionAsync(string prompt)
    {
        var model = this._configService.GeneralConfig.Ollama.Model;
        var response = await this._client.Completions.GenerateCompletionAsync(model, prompt, stream: false).WaitAsync();
        return response.Response ?? string.Empty;
    }
}
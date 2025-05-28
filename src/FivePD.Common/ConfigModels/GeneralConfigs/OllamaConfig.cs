namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record OllamaConfig
{
    /// <summary>
    /// Gets the model name to use for Ollama completions.
    /// </summary>
    public string Model { get; init; }
    
    /// <summary>
    /// Gets the language to use for Ollama completions.
    /// </summary>
    public string Language { get; init; }

    /// <summary>
    /// Gets the URL of the Ollama server to connect to.
    /// </summary>
    public string Url { get; init; }
}
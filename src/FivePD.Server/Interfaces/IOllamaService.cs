using System.Threading.Tasks;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides access to the Ollama API for generating AI completions.
/// This service is used to interact with the Ollama API in a centralized way,
/// allowing for consistent model usage and configuration across the application.
/// </summary>
public interface IOllamaService
{
    /// <summary>
    /// Gets a completion from the Ollama model for the given prompt.
    /// </summary>
    /// <param name="prompt">The prompt to send to the model.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the generated text response from the model, or an empty string if no response was generated.
    /// </returns>
    /// <remarks>
    /// This method uses the model specified in the general config file under the Ollama section.
    /// The response is generated synchronously (non-streaming) for simplicity and reliability.
    /// </remarks>
    Task<string> GetCompletionAsync(string prompt);
}
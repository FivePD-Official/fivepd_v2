using System.Collections.Generic;

namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record QuestioningConfig
{
    /// <summary>
    /// Gets the list of available questions that can be asked peds.
    /// </summary>
    public List<QuestionConfig> Questions { get; init; } = new();

    /// <summary>
    /// Gets whether to use Ollama for generating responses.
    /// If false, responses will be taken from the config file.
    /// </summary>
    public bool UseOllamaResponses { get; init; }

    public sealed record QuestionConfig
    {
        /// <summary>
        /// Gets the unique identifier for the question.
        /// </summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>
        /// Gets the text of the question to ask.
        /// </summary>
        public string Text { get; init; } = string.Empty;

        /// <summary>
        /// Gets the list of possible responses if not using Ollama.
        /// Each response has a chance to be selected based on the ped's state.
        /// </summary>
        public List<ResponseConfig> Responses { get; init; } = new();
    }

    public sealed record ResponseConfig
    {
        /// <summary>
        /// Gets the text of the response.
        /// </summary>
        public string Text { get; init; } = string.Empty;

        /// <summary>
        /// Gets the conditions under which this response should be selected.
        /// </summary>
        public ResponseConditions Conditions { get; init; } = new();

        /// <summary>
        /// Gets the chance (0-100) of this response being selected when conditions are met.
        /// </summary>
        public int Chance { get; init; } = 100;
    }

    public sealed record ResponseConditions
    {
        /// <summary>
        /// Gets whether the ped has any illegal items.
        /// </summary>
        public bool HasIllegalItems { get; init; }

        /// <summary>
        /// Gets whether the ped is over the legal alcohol limit.
        /// </summary>
        public bool IsOverAlcoholLimit { get; init; }

        /// <summary>
        /// Gets whether the ped has any drugs in their system.
        /// </summary>
        public bool HasDrugs { get; init; }
    }
} 
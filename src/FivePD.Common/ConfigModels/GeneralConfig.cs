using FivePD.Common.ConfigModels.GeneralConfigs;

namespace FivePD.Common.ConfigModels;

#pragma warning disable

internal interface IInternalGeneralConfig
{
    public DatabaseConfig Database { get; set; }
}

public sealed record GeneralConfig : IInternalGeneralConfig
{
    private DatabaseConfig _database = new();

    public DatabaseConfig Database
    {
        get => this._database;
        init => this._database = value;
    }

    DatabaseConfig IInternalGeneralConfig.Database
    {
        get => this._database;
        set => this._database = value;
    }

    public string DefaultLanguage { get; init; } = string.Empty;
    
    public AudioConfig Audio { get; init; } = new();

    public UnitConfig Unit { get; init; } = new();

    public CalloutConfig Callout { get; init; } = new();

    public AssistanceServicesConfig AssistanceServices { get; init; } = new();

    public FeatureFlagsConfig Features { get; init; } = new();

    public ProgressionSystemConfig ProgressionSystem { get; init; } = new();

    public PedsConfig Peds { get; init; } = new();

    public VehiclesConfig Vehicles { get; init; } = new();

    public MessagesConfig Messages { get; init; } = new();

    public MdtConfig Mdt { get; init; } = new();

    public MenusConfig Menu { get; init; } = new();

    public OllamaConfig Ollama { get; init; } = new();

    public QuestioningConfig Questioning { get; init; } = new();
}
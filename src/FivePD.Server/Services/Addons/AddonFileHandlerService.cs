using System;
using System.Collections.Generic;
using FivePD.Server.Attributes;
using FivePD.Server.Interfaces.Addons;
using Newtonsoft.Json;
using Serilog;
using YamlDotNet.Serialization;

namespace FivePD.Server.Services.Addons;

/// <inheritdoc />
[Obfuscate]
public class AddonFileHandlerService : IAddonFileHandlerService
{
    private readonly IInternalAddonMetadataService _addonMetadataService;
    private readonly ILogger _logger;
    private readonly Dictionary<string, Dictionary<string, object>> _deserializedFileCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddonFileHandlerService"/> class.
    /// </summary>
    /// <param name="addonMetadataService"> The <see cref="IAddonMetadataService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public AddonFileHandlerService(IAddonMetadataService addonMetadataService, ILogger logger)
    {
        this._addonMetadataService = (IInternalAddonMetadataService)addonMetadataService;
        this._logger = logger;
    }

    /// <inheritdoc />
    public string LoadFile(string path)
    {
        var addonDefinition = this._addonMetadataService.GetExecutingAddonDefinition();
        return addonDefinition is null ? string.Empty : addonDefinition.Files[path];
    }

    /// <inheritdoc />
    public T LoadFile<T>(string path)
    {
        try
        {
            var addonDefinition = this._addonMetadataService.GetExecutingAddonDefinition();
            if (addonDefinition is null)
            {
                return Activator.CreateInstance<T>();
            }

            if (this._deserializedFileCache.TryGetValue(addonDefinition.Name, out var fileCache))
            {
                if (fileCache.TryGetValue(path, out var deserializedFile))
                {
                    return (T)deserializedFile;
                }
            }
            else
            {
                fileCache = this._deserializedFileCache[addonDefinition.Name] = new Dictionary<string, object>();
            }

            var content = addonDefinition.Files[path];
            T deserializedObject;
            if (path.EndsWith(".json"))
            {
                deserializedObject = JsonConvert.DeserializeObject<T>(content);
                fileCache[path] = deserializedObject;
            }
            else if (path.EndsWith(".yaml"))
            {
                var serializer = new DeserializerBuilder().Build();
                deserializedObject = serializer.Deserialize<T>(string.IsNullOrEmpty(content) || content.Length <= 1 ? "." : content);
            }
            else
            {
                deserializedObject = Activator.CreateInstance<T>();
            }

            return deserializedObject;
        }
        catch (Exception e)
        {
            this._logger.Error(
                "An error occured while parsing the {Path} file, the default values will be used. Make sure that the JSON or YAML formatting is correct. Exception: {Exception}",
                path,
                e);
            return Activator.CreateInstance<T>();
        }
    }
}
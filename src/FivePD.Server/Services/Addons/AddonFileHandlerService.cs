using System;
using System.Collections.Generic;
using FivePD.Server.Interfaces.Addons;
using Newtonsoft.Json;

namespace FivePD.Server.Services.Addons;

/// <inheritdoc />
public class AddonFileHandlerService : IAddonFileHandlerService
{
    private readonly IAddonMetadataService _addonMetadataService;
    private readonly Dictionary<string, Dictionary<string, object>> _deserializedFileCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddonFileHandlerService"/> class.
    /// </summary>
    /// <param name="addonMetadataService"> The <see cref="IAddonMetadataService"/> to use.</param>
    public AddonFileHandlerService(IAddonMetadataService addonMetadataService)
    {
        this._addonMetadataService = addonMetadataService;
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

            try
            {
                var deserializedObject = JsonConvert.DeserializeObject<T>(addonDefinition.Files[path]);
                fileCache[path] = deserializedObject;

                return deserializedObject;
            }
            catch (Exception)
            {
                return Activator.CreateInstance<T>();
            }
        }
        catch (Exception)
        {
            return default;
        }
    }
}
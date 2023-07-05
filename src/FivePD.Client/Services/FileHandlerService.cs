using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Common;
using Newtonsoft.Json;

namespace FivePD.Client.Services;

/// <inheritdoc />
internal class FileHandlerService : IFileHandlerService
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    /// <inheritdoc />
    public async Task<T> LoadFileAsync<T>(string path)
    {
        if (this._cache.TryGetValue(path, out var value))
        {
            return (T)value;
        }

        var gotResponse = false;
        var deserializedObject = default(T);

        BaseScript.TriggerServerEvent(Events.Config.Get, path, new Action<string>(data =>
        {
            if (string.IsNullOrEmpty(data))
            {
                data = "{}";
            }

            try
            {
                deserializedObject = JsonConvert.DeserializeObject<T>(data);
                this._cache[path] = deserializedObject;
            }
            catch (Exception)
            {
                deserializedObject = Activator.CreateInstance<T>();
            }

            gotResponse = true;
        }));

        while (!gotResponse)
        {
            await BaseScript.Delay(100);
        }

        return deserializedObject;
    }
}
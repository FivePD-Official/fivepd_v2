using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FivePD.Common.Exceptions;
using FivePD.Server.Interfaces;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class FileHandlerService : IFileHandlerService
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    /// <inheritdoc />
    public List<string> LoadStringList(string path)
    {
        var data = this.InternalLoadFile(path);

        if (this._cache.TryGetValue(path, out var value))
        {
            return (List<string>)value;
        }

        if (string.IsNullOrEmpty(data.Item2))
        {
            return new List<string>();
        }

        var list = data.Item2.Split('\n').ToList();
        this._cache[path] = list;

        return list;
    }

    /// <inheritdoc />
    public Tuple<T, Exception> LoadFile<T>(string path, bool useCache = true)
    {
        if (this._cache.ContainsKey(path) && useCache)
        {
            return new Tuple<T, Exception>((T)this._cache[path], null);
        }

        Exception exception = null;
        var data = this.InternalLoadFile(path);
        var content = data.Item2;

        if (string.IsNullOrEmpty(content))
        {
            content = data.Item1 ? "{}" : string.Empty;
            exception = new EmptyConfigException();
        }

        try
        {
            T deserializedObject;
            if (data.Item1)
            {
                deserializedObject = JsonConvert.DeserializeObject<T>(content);
            }
            else
            {
                var serializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                deserializedObject = serializer.Deserialize<T>(string.IsNullOrEmpty(content) || content.Length <= 1 ? "." : content);
            }

            if (useCache)
            {
                this._cache[path] = deserializedObject;
            }

            return new Tuple<T, Exception>(deserializedObject, exception);
        }
        catch (Exception e)
        {
            exception = e;
        }

        return new Tuple<T, Exception>(Activator.CreateInstance<T>(), exception);
    }

    /// <inheritdoc />
    public Tuple<bool, string> LoadFile(string path)
    {
        return this.InternalLoadFile(path);
    }

    /// <summary>
    /// Loads a file's content then returns it without validation.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>A tuple with a bool specifying whether the file is JSON or not and the actual content.</returns>
    private Tuple<bool, string> InternalLoadFile(string path)
    {
        if (path.Contains(".txt"))
        {
            return Tuple.Create(false, Cfx.API.LoadResourceFile(Cfx.API.GetCurrentResourceName(), path));
        }

        var contentJson = Cfx.API.LoadResourceFile(Cfx.API.GetCurrentResourceName(), $"{path}.json");
        var contentYaml = Cfx.API.LoadResourceFile(Cfx.API.GetCurrentResourceName(), $"{path}.yaml");

        return string.IsNullOrEmpty(contentJson) ? Tuple.Create(false, contentYaml) : Tuple.Create(true, contentJson);
    }
}
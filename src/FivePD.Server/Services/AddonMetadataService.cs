using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FivePD.Server.API;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IAddonMetadataService" />
public class AddonMetadataService : IAddonMetadataService, IInternalAddonMetadataService
{
    private readonly Dictionary<Assembly, AddonDefinition> _definitions = new();
    private readonly List<IExtension> _extensions = new();
    private readonly List<CalloutType> _calloutTypes = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddonMetadataService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="serviceProvider">The <see cref="ServiceProvider"/> to use.</param>
    public AddonMetadataService(IServiceProvider serviceProvider, ILogger logger)
    {
        this._serviceProvider = serviceProvider;
        this._logger = logger;
    }

    /// <inheritdoc />
    public ReadOnlyCollection<Type> CalloutTypes => this._calloutTypes.Select(type => type.Type).ToList().AsReadOnly();

    /// <inheritdoc />
    ReadOnlyCollection<CalloutType> IInternalAddonMetadataService.CalloutTypes => this._calloutTypes.AsReadOnly();

    /// <inheritdoc/>
    List<IExtension> IInternalAddonMetadataService.Extensions => this._extensions;

    /// <inheritdoc/>
    Callout IInternalAddonMetadataService.InstantiateCallout(Type type)
    {
        var parameters = type.GetConstructors()[0].GetParameters();
        var callout = (Callout)Activator.CreateInstance(
            type,
            parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray());

        return callout;
    }

    /// <inheritdoc />
    public AddonDefinition GetExecutingAddonDefinition()
    {
        // Technically, the executing assembly is the server. Sort through the stack trace and find a ref. to the actual addon
        foreach (var frame in new StackTrace().GetFrames())
        {
            try
            {
                if (frame.GetMethod().DeclaringType!.Assembly != Assembly.GetExecutingAssembly())
                {
                    var declaringTypeAssembly = frame.GetMethod().DeclaringType?.Assembly;
                    if (declaringTypeAssembly is not null)
                    {
                        return this._definitions[declaringTypeAssembly];
                    }
                }
            }
            catch (Exception)
            {
                // Continues when frame assembly is null
            }
        }

        return null;
    }

    /// <inheritdoc />
    public void LoadAssemblies(List<AddonDefinition> definitions)
    {
        foreach (var addon in definitions)
        {
            this._logger.Information(
                "Registering addon \"{@AddonName}\" version {@AddonVersion}",
                addon.FriendlyName,
                addon.FriendlyVersion);

            foreach (var assembly in addon.Assemblies)
            {
                var asm = Assembly.Load(assembly);
                this._definitions.Add(asm, addon);
            }
        }
    }

    /// <inheritdoc />
    public void FindAndRegisterAddons()
    {
        foreach (var assembly in this._definitions.Keys)
        {
            // Find and register extensions
            var extensionTypes = assembly
                .GetTypes()
                .Where(type => type.IsAssignableFrom(typeof(IExtension)));
            foreach (var extensionType in extensionTypes)
            {
                try
                {
                    var parameters = extensionType.GetConstructors()[0].GetParameters();
                    var extension = Activator.CreateInstance(
                        extensionType,
                        parameters.Select(item => this._serviceProvider.GetService(item.ParameterType)).ToArray()) as IExtension;
                    if (extension is null)
                    {
                        this._logger.Error("Failed to load extension {Extension}", extensionType.FullName);
                        continue;
                    }

                    this._extensions.Add(extension);
                }
                catch (Exception)
                {
                    this._logger.Error("Failed to load extension {Extension}", extensionType.FullName);
                }
            }

            // Find and register callouts
            var calloutsTypes = assembly
                .GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Callout)))
                .Select(type => new CalloutType(type, type.Assembly.GetName().Name));
            this._calloutTypes.AddRange(calloutsTypes);
        }
    }
}